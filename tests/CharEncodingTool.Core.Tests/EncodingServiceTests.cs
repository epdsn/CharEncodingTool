using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class EncodingServiceTests
{
    [Fact]
    public void Ascii_BytesMatchExpected_ForPureAsciiInput()
    {
        var result = EncodingService.Encode("Hello", EncodingCatalog.GetById("ascii"));
        Assert.Equal(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, result.Bytes);
        Assert.Equal(5, result.ByteCount);
    }

    [Fact]
    public void Ascii_ReplacesNonAsciiWithQuestionMark()
    {
        var result = EncodingService.Encode("é", EncodingCatalog.GetById("ascii"));
        Assert.Equal(new byte[] { 0x3F }, result.Bytes);
    }

    [Fact]
    public void Utf8_AsciiInput_ProducesSameBytesAsAscii()
    {
        var ascii = EncodingService.Encode("Hello", EncodingCatalog.GetById("ascii"));
        var utf8 = EncodingService.Encode("Hello", EncodingCatalog.GetById("utf8"));
        Assert.Equal(ascii.Bytes, utf8.Bytes);
    }

    [Fact]
    public void Utf8_TwoByteSequence_ForLatinAccented()
    {
        var result = EncodingService.Encode("é", EncodingCatalog.GetById("utf8"));
        Assert.Equal(new byte[] { 0xC3, 0xA9 }, result.Bytes);
    }

    [Fact]
    public void Utf8_FourByteSequence_ForSupplementaryPlane()
    {
        var result = EncodingService.Encode("🎉", EncodingCatalog.GetById("utf8"));
        Assert.Equal(new byte[] { 0xF0, 0x9F, 0x8E, 0x89 }, result.Bytes);
    }

    [Fact]
    public void Utf8WithBom_PrefixesEfBbBf()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf8-bom"));
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF, 0x41 }, result.Bytes);
    }

    [Fact]
    public void Utf16Le_AsciiChar_HasLowByteFirst()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf16-le"));
        Assert.Equal(new byte[] { 0x41, 0x00 }, result.Bytes);
    }

    [Fact]
    public void Utf16Be_AsciiChar_HasHighByteFirst()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf16-be"));
        Assert.Equal(new byte[] { 0x00, 0x41 }, result.Bytes);
    }

    [Fact]
    public void Utf16Le_Emoji_UsesSurrogatePair()
    {
        var result = EncodingService.Encode("🎉", EncodingCatalog.GetById("utf16-le"));
        // U+1F389 → high surrogate D83C, low surrogate DF89 (LE byte order)
        Assert.Equal(new byte[] { 0x3C, 0xD8, 0x89, 0xDF }, result.Bytes);
    }

    [Fact]
    public void Utf16LeWithBom_PrefixesFfFe()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf16-le-bom"));
        Assert.Equal(new byte[] { 0xFF, 0xFE, 0x41, 0x00 }, result.Bytes);
    }

    [Fact]
    public void Utf16BeWithBom_PrefixesFeFf()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf16-be-bom"));
        Assert.Equal(new byte[] { 0xFE, 0xFF, 0x00, 0x41 }, result.Bytes);
    }

    [Fact]
    public void Utf32Le_AlwaysFourBytesPerCodePoint()
    {
        var result = EncodingService.Encode("A🎉", EncodingCatalog.GetById("utf32-le"));
        // 'A' = U+0041 → 41 00 00 00, '🎉' = U+1F389 → 89 F3 01 00
        Assert.Equal(new byte[] { 0x41, 0x00, 0x00, 0x00, 0x89, 0xF3, 0x01, 0x00 }, result.Bytes);
    }

    [Fact]
    public void Utf32Be_FourBytesHighByteFirst()
    {
        var result = EncodingService.Encode("A", EncodingCatalog.GetById("utf32-be"));
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x41 }, result.Bytes);
    }

    [Theory]
    [InlineData("utf8")]
    [InlineData("utf8-bom")]
    [InlineData("utf16-le")]
    [InlineData("utf16-be")]
    [InlineData("utf16-le-bom")]
    [InlineData("utf16-be-bom")]
    [InlineData("utf32-le")]
    [InlineData("utf32-be")]
    public void RoundTrip_StringToBytesToString(string id)
    {
        var descriptor = EncodingCatalog.GetById(id);
        const string input = "Hello, 世界! 🎉";
        var encoded = EncodingService.Encode(input, descriptor);
        var decoded = EncodingService.Decode(encoded.Bytes, descriptor);
        Assert.Equal(input, decoded);
    }

    [Fact]
    public void Encode_EmptyString_ProducesEmptyBytes_NoBomForNoBomEncoding()
    {
        var result = EncodingService.Encode(string.Empty, EncodingCatalog.GetById("utf8"));
        Assert.Empty(result.Bytes);
    }

    [Fact]
    public void Encode_EmptyString_StillEmitsBom_WhenEncodingIsBomFlavour()
    {
        var result = EncodingService.Encode(string.Empty, EncodingCatalog.GetById("utf8-bom"));
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, result.Bytes);
    }
}

public class ByteFormatterTests
{
    [Fact]
    public void ToHexSpaced_ProducesUppercaseSpaceSeparated()
    {
        Assert.Equal("48 65 6C 6C 6F", ByteFormatter.ToHexSpaced("Hello"u8));
    }

    [Fact]
    public void ToHexCompact_NoSeparators()
    {
        Assert.Equal("48656C6C6F", ByteFormatter.ToHexCompact("Hello"u8));
    }

    [Fact]
    public void ToBase64_MatchesConvertOutput()
    {
        Assert.Equal("SGVsbG8=", ByteFormatter.ToBase64("Hello"u8));
    }

    [Fact]
    public void ToPercentEncoded_EveryBytePercentPrefixed()
    {
        Assert.Equal("%48%65%6C%6C%6F", ByteFormatter.ToPercentEncoded("Hello"u8));
    }

    [Theory]
    [InlineData("48 65 6C 6C 6F")]
    [InlineData("48656C6C6F")]
    [InlineData("0x48, 0x65, 0x6C, 0x6C, 0x6F")]
    [InlineData("48-65-6C-6C-6F")]
    [InlineData("\\x48\\x65\\x6C\\x6C\\x6F")]
    public void ParseHex_AcceptsCommonFormats(string input)
    {
        Assert.Equal(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, ByteFormatter.ParseHex(input));
    }

    [Fact]
    public void ParseHex_RejectsOddLength()
    {
        Assert.Throws<FormatException>(() => ByteFormatter.ParseHex("486"));
    }

    [Fact]
    public void ParseBase64_RoundTripsWithToBase64()
    {
        var bytes = ByteFormatter.ParseBase64("SGVsbG8=");
        Assert.Equal("Hello"u8.ToArray(), bytes);
    }
}
