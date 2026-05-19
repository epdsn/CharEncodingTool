using System.Globalization;
using CharEncodingTool.Core.Models;

namespace CharEncodingTool.Core.Services;

public static class CodePointAnalyzer
{
    public static IReadOnlyList<CodePointInfo> Analyze(string input)
    {
        if (string.IsNullOrEmpty(input)) return [];

        var utf8 = EncodingCatalog.GetById("utf8").Encoding;
        var utf16Le = EncodingCatalog.GetById("utf16-le").Encoding;
        var utf16Be = EncodingCatalog.GetById("utf16-be").Encoding;
        var utf32Le = EncodingCatalog.GetById("utf32-le").Encoding;

        var result = new List<CodePointInfo>(input.Length);
        var enumerator = StringInfo.GetTextElementEnumerator(input);

        // We iterate by UTF-32 code points (not text elements / grapheme clusters) because
        // the user wants to see each code point, including individual combining marks.
        for (int i = 0; i < input.Length;)
        {
            int codePoint = char.ConvertToUtf32(input, i);
            int charsConsumed = char.IsHighSurrogate(input[i]) ? 2 : 1;
            string glyph = char.ConvertFromUtf32(codePoint);

            result.Add(new CodePointInfo(
                Glyph: glyph,
                GlyphDisplay: ControlCharRenderer.Render(glyph),
                CodePoint: codePoint,
                UnicodeNotation: $"U+{codePoint:X4}",
                Category: CharUnicodeInfo.GetUnicodeCategory(codePoint),
                Utf8Hex:    ByteFormatter.ToHexSpaced(utf8.GetBytes(glyph)),
                Utf16LeHex: ByteFormatter.ToHexSpaced(utf16Le.GetBytes(glyph)),
                Utf16BeHex: ByteFormatter.ToHexSpaced(utf16Be.GetBytes(glyph)),
                Utf32LeHex: ByteFormatter.ToHexSpaced(utf32Le.GetBytes(glyph)),
                Utf8Bits:   Utf8BitPattern.For(codePoint)));

            i += charsConsumed;
        }
        _ = enumerator; // StringInfo enumerator unused; kept the include in case grapheme view is added later.
        return result;
    }
}
