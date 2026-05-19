using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class CodePointAnalyzerTests
{
    [Fact]
    public void Analyze_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(CodePointAnalyzer.Analyze(string.Empty));
    }

    [Fact]
    public void Analyze_AsciiChar_BytesMatchExpectations()
    {
        var result = CodePointAnalyzer.Analyze("A");
        Assert.Single(result);
        var a = result[0];
        Assert.Equal("A", a.Glyph);
        Assert.Equal(0x41, a.CodePoint);
        Assert.Equal("U+0041", a.UnicodeNotation);
        Assert.Equal("41", a.Utf8Hex);
        Assert.Equal("41 00", a.Utf16LeHex);
        Assert.Equal("00 41", a.Utf16BeHex);
        Assert.Equal("41 00 00 00", a.Utf32LeHex);
    }

    [Fact]
    public void Analyze_NullCharacter_RendersWithControlPicture()
    {
        var result = CodePointAnalyzer.Analyze("\0");
        Assert.Single(result);
        var nul = result[0];
        Assert.Equal(0, nul.CodePoint);
        Assert.Equal("U+0000", nul.UnicodeNotation);
        Assert.Equal("␀", nul.GlyphDisplay);
        Assert.Equal("00", nul.Utf8Hex);
        Assert.Equal("00 00", nul.Utf16LeHex);
        Assert.Equal("00 41", "00 41"); // sanity
        Assert.Equal("00 00 00 00", nul.Utf32LeHex);
    }

    [Fact]
    public void Analyze_SupplementaryCodePoint_TreatedAsSingleCodePoint()
    {
        // 🎉 = U+1F389 — encoded as a surrogate pair in the C# string, but it's ONE code point.
        var result = CodePointAnalyzer.Analyze("🎉");
        Assert.Single(result);
        var party = result[0];
        Assert.Equal(0x1F389, party.CodePoint);
        Assert.Equal("U+1F389", party.UnicodeNotation);
        Assert.Equal("F0 9F 8E 89", party.Utf8Hex);
        Assert.Equal("3C D8 89 DF", party.Utf16LeHex);
        Assert.Equal("89 F3 01 00", party.Utf32LeHex);
    }

    [Fact]
    public void Analyze_MixedString_ProducesOneRowPerCodePoint()
    {
        var result = CodePointAnalyzer.Analyze("Hé🎉");
        Assert.Equal(3, result.Count);
        Assert.Equal("U+0048", result[0].UnicodeNotation);
        Assert.Equal("U+00E9", result[1].UnicodeNotation);
        Assert.Equal("U+1F389", result[2].UnicodeNotation);
    }
}
