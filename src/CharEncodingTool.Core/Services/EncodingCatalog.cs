using System.Text;
using CharEncodingTool.Core.Models;

namespace CharEncodingTool.Core.Services;

public static class EncodingCatalog
{
    public static IReadOnlyList<EncodingDescriptor> All { get; } = BuildCatalog();

    public static EncodingDescriptor GetById(string id) =>
        All.FirstOrDefault(e => e.Id == id)
        ?? throw new ArgumentException($"Unknown encoding id '{id}'", nameof(id));

    private static IReadOnlyList<EncodingDescriptor> BuildCatalog()
    {
        // .NET only ships ASCII / UTF-8 / UTF-16 / UTF-32 by default. Single-byte legacy code
        // pages (Windows-1252, ISO-8859-1, IBM437, etc.) require this provider to be registered.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // ASCII: 7-bit, single byte per char. Anything > U+007F replaced with '?'.
        var ascii = Encoding.ASCII;

        // UTF-8 in two flavours: BOM (EF BB BF prefix) and no-BOM. Most APIs want no-BOM.
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
        var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: false);

        // UTF-16 with explicit byte order. ctor: (bigEndian, byteOrderMark)
        var utf16Le = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
        var utf16Be = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
        var utf16LeBom = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
        var utf16BeBom = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);

        // UTF-32 with explicit byte order. ctor: (bigEndian, byteOrderMark)
        var utf32Le = new UTF32Encoding(bigEndian: false, byteOrderMark: false);
        var utf32Be = new UTF32Encoding(bigEndian: true, byteOrderMark: false);

        // Single-byte legacy code pages. Each maps 256 byte values to specific Unicode code points.
        var windows1252 = Encoding.GetEncoding(1252);
        var iso88591   = Encoding.GetEncoding(28591);
        var ibm437     = Encoding.GetEncoding(437);

        return new List<EncodingDescriptor>
        {
            new("ascii",        "ASCII",            ascii,        "7-bit. Any code point > 0x7F is replaced with '?'."),
            new("utf8",         "UTF-8 (no BOM)",   utf8NoBom,    "Variable length 1–4 bytes. ASCII bytes round-trip unchanged. The default for most APIs and the web."),
            new("utf8-bom",     "UTF-8 (with BOM)", utf8WithBom,  "Prefixes the bytes EF BB BF. Some tools require it; most modern APIs reject it."),
            new("utf16-le",     "UTF-16 LE",        utf16Le,      "2 bytes per BMP code point, 4 bytes via surrogate pair for code points ≥ U+10000. Little-endian: low byte first."),
            new("utf16-be",     "UTF-16 BE",        utf16Be,      "Same as UTF-16 LE but high byte first. Network byte order."),
            new("utf16-le-bom", "UTF-16 LE + BOM",  utf16LeBom,   "Prefixes FF FE. Identifies endianness for downstream readers."),
            new("utf16-be-bom", "UTF-16 BE + BOM",  utf16BeBom,   "Prefixes FE FF."),
            new("utf32-le",     "UTF-32 LE",        utf32Le,      "4 bytes per code point, always. Wasteful but trivial to index."),
            new("utf32-be",     "UTF-32 BE",        utf32Be,      "Same as UTF-32 LE but high byte first."),
            new("windows-1252", "Windows-1252",     windows1252,  "Single-byte. ASCII for 0x00–0x7F, Western European Latin + smart quotes + € in 0x80–0xFF. The default on Windows before UTF-8 took over. Often mislabelled as ISO-8859-1."),
            new("iso-8859-1",   "ISO-8859-1",       iso88591,     "Single-byte (Latin-1). ASCII for 0x00–0x7F, Western European Latin in 0xA0–0xFF. 0x80–0x9F are C1 control codes (no €, no smart quotes). Code points map 1:1 to U+0000–U+00FF."),
            new("ibm437",       "IBM CP437 (DOS)",  ibm437,       "The original IBM PC code page. ASCII for 0x00–0x7F, box-drawing + Greek + math symbols in 0x80–0xFF. Still used for COM-port output on legacy hardware."),
        };
    }
}
