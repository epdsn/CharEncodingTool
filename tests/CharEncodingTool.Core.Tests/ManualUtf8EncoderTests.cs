using System.Text;
using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class ManualUtf8EncoderTests
{
    // ---------- One-byte range (ASCII) ----------

    [Fact]
    public void Encode_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(ManualUtf8Encoder.Encode(string.Empty));
    }

    [Theory]
    [InlineData("A", new byte[] { 0x41 })]
    [InlineData("Hello", new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F })]
    [InlineData("\0", new byte[] { 0x00 })]
    [InlineData("\x7F", new byte[] { 0x7F })]
    public void Encode_AsciiRange_OneBytePerChar(string input, byte[] expected)
    {
        Assert.Equal(expected, ManualUtf8Encoder.Encode(input));
    }

    // ---------- Two-byte range (U+0080 – U+07FF) ----------

    [Theory]
    [InlineData("", new byte[] { 0xC2, 0x80 })]   // first 2-byte code point
    [InlineData("é",     new byte[] { 0xC3, 0xA9 })]   // U+00E9
    [InlineData("ñ",     new byte[] { 0xC3, 0xB1 })]   // U+00F1
    [InlineData("߿", new byte[] { 0xDF, 0xBF })]   // last 2-byte code point
    public void Encode_TwoByteRange_MatchesSpec(string input, byte[] expected)
    {
        Assert.Equal(expected, ManualUtf8Encoder.Encode(input));
    }

    // ---------- Three-byte range (U+0800 – U+FFFF) ----------

    [Theory]
    [InlineData("ࠀ", new byte[] { 0xE0, 0xA0, 0x80 })]   // first 3-byte code point
    [InlineData("€",     new byte[] { 0xE2, 0x82, 0xAC })]   // U+20AC
    [InlineData("世",     new byte[] { 0xE4, 0xB8, 0x96 })]   // U+4E16
    [InlineData("�", new byte[] { 0xEF, 0xBF, 0xBD })]   // replacement char
    [InlineData("￿", new byte[] { 0xEF, 0xBF, 0xBF })]   // last BMP code point
    public void Encode_ThreeByteRange_MatchesSpec(string input, byte[] expected)
    {
        Assert.Equal(expected, ManualUtf8Encoder.Encode(input));
    }

    // ---------- Four-byte range (U+10000 – U+10FFFF) ----------

    [Theory]
    [InlineData("🎉",        new byte[] { 0xF0, 0x9F, 0x8E, 0x89 })]   // U+1F389
    [InlineData("🚀",        new byte[] { 0xF0, 0x9F, 0x9A, 0x80 })]   // U+1F680
    [InlineData("\U00010000", new byte[] { 0xF0, 0x90, 0x80, 0x80 })]   // first 4-byte code point
    [InlineData("\U0010FFFF", new byte[] { 0xF4, 0x8F, 0xBF, 0xBF })]   // last valid Unicode code point
    public void Encode_FourByteRange_MatchesSpec(string input, byte[] expected)
    {
        Assert.Equal(expected, ManualUtf8Encoder.Encode(input));
    }

    // ---------- Surrogate handling ----------

    [Fact]
    public void Encode_LoneHighSurrogate_EmitsReplacementChar()
    {
        // High surrogate D83C with nothing following — invalid in well-formed Unicode.
        string input = "\uD83C";
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD }, ManualUtf8Encoder.Encode(input));
    }

    [Fact]
    public void Encode_LoneLowSurrogate_EmitsReplacementChar()
    {
        // Low surrogate DF89 without a preceding high surrogate.
        string input = "\uDF89";
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD }, ManualUtf8Encoder.Encode(input));
    }

    [Fact]
    public void Encode_HighSurrogateFollowedByNonLowSurrogate_EmitsReplacementThenChar()
    {
        // Two lone surrogates in a row: each becomes its own U+FFFD.
        string input = "\uD83CA";
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD, 0x41 }, ManualUtf8Encoder.Encode(input));
    }

    // ---------- Mixed content and length verification ----------

    [Fact]
    public void Encode_MixedString_HasExpectedByteCount()
    {
        // H(1) + é(2) + 世(3) + 🎉(4) = 10 bytes total
        var bytes = ManualUtf8Encoder.Encode("Hé世🎉");
        Assert.Equal(10, bytes.Length);
        Assert.Equal(new byte[]
        {
            0x48,                         // 'H'
            0xC3, 0xA9,                   // 'é'
            0xE4, 0xB8, 0x96,             // '世'
            0xF0, 0x9F, 0x8E, 0x89,       // '🎉'
        }, bytes);
    }

    // ---------- Parity sweep against .NET's built-in encoder ----------
    //
    // This is the load-bearing test. If we can match the framework's output across this many
    // strings — including supplementary planes and surrogate junk — the implementation is right.

    public static IEnumerable<object[]> ParityInputs => new[]
    {
        new object[] { "" },
        new object[] { "Hello, world!" },
        new object[] { "The quick brown fox jumps over the lazy dog." },
        new object[] { "Café résumé naïve coöperate" },
        new object[] { "Hé世🎉" },
        new object[] { "私は猫" },
        new object[] { "Здравствуй" },
        new object[] { "مرحبا" },
        new object[] { "👨‍👩‍👧‍👦" },         // family emoji (multiple code points joined by ZWJs)
        new object[] { "🇺🇸🇯🇵" },           // flag emoji (regional indicator pairs)
        new object[] { "\0\x01\x02\x7F" },     // C0 controls and DEL
        new object[] { "\uD83C" },             // lone high surrogate
        new object[] { "\uDF89" },             // lone low surrogate
        new object[] { "\uD83CA\uDF89" },      // surrogates separated by ASCII
        new object[] { "߿ࠀ￿" }, // range boundaries
        new object[] { "\U00010000\U0010FFFF" },     // 4-byte range boundaries
    };

    [Theory]
    [MemberData(nameof(ParityInputs))]
    public void Encode_MatchesDotNetUtf8(string input)
    {
        var ours = ManualUtf8Encoder.Encode(input);
        var theirs = Encoding.UTF8.GetBytes(input);
        Assert.Equal(theirs, ours);
    }
}
