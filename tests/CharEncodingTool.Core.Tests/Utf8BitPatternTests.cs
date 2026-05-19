using System.Text;
using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class Utf8BitPatternTests
{
    [Fact]
    public void For_AsciiChar_SingleByteWithLeadingZero()
    {
        // 'A' = U+0041 = 0b1000001
        var pattern = Utf8BitPattern.For(0x41);
        Assert.Single(pattern);
        Assert.Equal("0",       pattern[0].MarkerBits);
        Assert.Equal("1000001", pattern[0].PayloadBits);
    }

    [Fact]
    public void For_NullCodePoint_StillSingleByte()
    {
        var pattern = Utf8BitPattern.For(0x00);
        Assert.Single(pattern);
        Assert.Equal("0",       pattern[0].MarkerBits);
        Assert.Equal("0000000", pattern[0].PayloadBits);
    }

    [Fact]
    public void For_LastOneByteCodePoint_U007F()
    {
        var pattern = Utf8BitPattern.For(0x7F);
        Assert.Single(pattern);
        Assert.Equal("0",       pattern[0].MarkerBits);
        Assert.Equal("1111111", pattern[0].PayloadBits);
    }

    [Fact]
    public void For_FirstTwoByteCodePoint_U0080()
    {
        var pattern = Utf8BitPattern.For(0x80);
        Assert.Equal(2, pattern.Count);
        Assert.Equal("110",    pattern[0].MarkerBits);
        Assert.Equal("00010",  pattern[0].PayloadBits);
        Assert.Equal("10",     pattern[1].MarkerBits);
        Assert.Equal("000000", pattern[1].PayloadBits);
    }

    [Fact]
    public void For_LatinAccented_E()
    {
        // 'é' = U+00E9 → C3 A9 → 11000011 10101001
        // Marker bits 110, 10  Payload 00011, 101001
        var pattern = Utf8BitPattern.For(0x00E9);
        Assert.Equal(2, pattern.Count);
        Assert.Equal("110",    pattern[0].MarkerBits);
        Assert.Equal("00011",  pattern[0].PayloadBits);
        Assert.Equal("10",     pattern[1].MarkerBits);
        Assert.Equal("101001", pattern[1].PayloadBits);
    }

    [Fact]
    public void For_EuroSign_ThreeByteSequence()
    {
        // '€' = U+20AC → E2 82 AC → 11100010 10000010 10101100
        var pattern = Utf8BitPattern.For(0x20AC);
        Assert.Equal(3, pattern.Count);
        Assert.Equal("1110",   pattern[0].MarkerBits);
        Assert.Equal("0010",   pattern[0].PayloadBits);
        Assert.Equal("10",     pattern[1].MarkerBits);
        Assert.Equal("000010", pattern[1].PayloadBits);
        Assert.Equal("10",     pattern[2].MarkerBits);
        Assert.Equal("101100", pattern[2].PayloadBits);
    }

    [Fact]
    public void For_PartyPopperEmoji_FourByteSequence()
    {
        // '🎉' = U+1F389 → F0 9F 8E 89 → 11110000 10011111 10001110 10001001
        var pattern = Utf8BitPattern.For(0x1F389);
        Assert.Equal(4, pattern.Count);
        Assert.Equal("11110",  pattern[0].MarkerBits);
        Assert.Equal("000",    pattern[0].PayloadBits);
        Assert.Equal("10",     pattern[1].MarkerBits);
        Assert.Equal("011111", pattern[1].PayloadBits);
        Assert.Equal("10",     pattern[2].MarkerBits);
        Assert.Equal("001110", pattern[2].PayloadBits);
        Assert.Equal("10",     pattern[3].MarkerBits);
        Assert.Equal("001001", pattern[3].PayloadBits);
    }

    [Fact]
    public void For_MaxCodePoint_U10FFFF()
    {
        // Last legal Unicode code point. UTF-8: F4 8F BF BF → 11110100 10001111 10111111 10111111
        var pattern = Utf8BitPattern.For(0x10FFFF);
        Assert.Equal(4, pattern.Count);
        Assert.Equal("11110",  pattern[0].MarkerBits);
        Assert.Equal("100",    pattern[0].PayloadBits);
        Assert.Equal("10",     pattern[3].MarkerBits);
        Assert.Equal("111111", pattern[3].PayloadBits);
    }

    // -------- Parity sweep --------

    [Theory]
    [InlineData(0x0041)]   // 'A'
    [InlineData(0x00E9)]   // 'é'
    [InlineData(0x4E16)]   // '世'
    [InlineData(0x20AC)]   // '€'
    [InlineData(0xFFFD)]   // replacement char
    [InlineData(0x1F389)]  // '🎉'
    [InlineData(0x10FFFF)] // max code point
    public void Reassembled_BitPattern_MatchesUtf8GetBytes(int codePoint)
    {
        var pattern = Utf8BitPattern.For(codePoint);
        var reassembled = new byte[pattern.Count];
        for (int i = 0; i < pattern.Count; i++)
        {
            string combined = pattern[i].MarkerBits + pattern[i].PayloadBits;
            Assert.Equal(8, combined.Length);
            reassembled[i] = Convert.ToByte(combined, 2);
        }
        var expected = Encoding.UTF8.GetBytes(char.ConvertFromUtf32(codePoint));
        Assert.Equal(expected, reassembled);
    }
}
