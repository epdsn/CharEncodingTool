using System.Globalization;

namespace CharEncodingTool.Core.Models;

public sealed record CodePointInfo(
    string Glyph,
    string GlyphDisplay,
    int CodePoint,
    string UnicodeNotation,
    UnicodeCategory Category,
    string Utf8Hex,
    string Utf16LeHex,
    string Utf16BeHex,
    string Utf32LeHex);
