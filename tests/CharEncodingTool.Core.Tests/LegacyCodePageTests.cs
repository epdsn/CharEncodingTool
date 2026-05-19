using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class LegacyCodePageTests
{
    [Fact]
    public void Windows1252_HasEuroAtByte0x80()
    {
        var result = EncodingService.Encode("€", EncodingCatalog.GetById("windows-1252"));
        Assert.Equal(new byte[] { 0x80 }, result.Bytes);
    }

    [Fact]
    public void Windows1252_HasSmartQuotesAt0x91To0x94()
    {
        // U+2018 ‘ → 0x91, U+2019 ’ → 0x92, U+201C “ → 0x93, U+201D ” → 0x94
        var result = EncodingService.Encode("‘’“”", EncodingCatalog.GetById("windows-1252"));
        Assert.Equal(new byte[] { 0x91, 0x92, 0x93, 0x94 }, result.Bytes);
    }

    [Fact]
    public void Iso88591_HasNoEuro_FallsBackToQuestionMark()
    {
        // ISO-8859-1 has no euro sign — encoding it should produce the replacement char.
        var result = EncodingService.Encode("€", EncodingCatalog.GetById("iso-8859-1"));
        Assert.Equal(new byte[] { 0x3F }, result.Bytes);
    }

    [Fact]
    public void Iso88591_AsciiInput_SameBytesAsAscii()
    {
        var result = EncodingService.Encode("Hello", EncodingCatalog.GetById("iso-8859-1"));
        Assert.Equal(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, result.Bytes);
    }

    [Fact]
    public void Iso88591_LatinAccented_OneBytePerChar()
    {
        // é (U+00E9) → 0xE9 in ISO-8859-1 (direct mapping)
        var result = EncodingService.Encode("é", EncodingCatalog.GetById("iso-8859-1"));
        Assert.Equal(new byte[] { 0xE9 }, result.Bytes);
    }

    [Fact]
    public void Cp437_AsciiInput_SameBytesAsAscii()
    {
        var result = EncodingService.Encode("Hello", EncodingCatalog.GetById("ibm437"));
        Assert.Equal(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, result.Bytes);
    }

    [Fact]
    public void Cp437_BoxDrawingChar_HasCorrectByte()
    {
        // ╔ (U+2554) → 0xC9 in CP437
        var result = EncodingService.Encode("╔", EncodingCatalog.GetById("ibm437"));
        Assert.Equal(new byte[] { 0xC9 }, result.Bytes);
    }

    [Fact]
    public void Cp437_AccentedChar_DifferentByteThanIso88591()
    {
        // é → 0x82 in CP437, 0xE9 in ISO-8859-1 — same character, different code page
        var cp437 = EncodingService.Encode("é", EncodingCatalog.GetById("ibm437"));
        var iso   = EncodingService.Encode("é", EncodingCatalog.GetById("iso-8859-1"));
        Assert.Equal(new byte[] { 0x82 }, cp437.Bytes);
        Assert.Equal(new byte[] { 0xE9 }, iso.Bytes);
    }

    [Theory]
    [InlineData("windows-1252")]
    [InlineData("iso-8859-1")]
    [InlineData("ibm437")]
    public void LegacyCodePage_RoundTripsForOwnAlphabet(string id)
    {
        var descriptor = EncodingCatalog.GetById(id);
        const string input = "Hello, world! 123.";
        var encoded = EncodingService.Encode(input, descriptor);
        var decoded = EncodingService.Decode(encoded.Bytes, descriptor);
        Assert.Equal(input, decoded);
    }
}
