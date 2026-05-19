using CharEncodingTool.Core.Models;

namespace CharEncodingTool.Core.Services;

public static class Utf8BitPattern
{
    public static IReadOnlyList<Utf8BitsByte> For(int codePoint)
    {
        if (codePoint < 0)
            throw new ArgumentOutOfRangeException(nameof(codePoint));

        // 1 byte: 0xxxxxxx                                7 payload bits
        // 2 byte: 110xxxxx 10xxxxxx                       5 + 6
        // 3 byte: 1110xxxx 10xxxxxx 10xxxxxx              4 + 6 + 6
        // 4 byte: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx     3 + 6 + 6 + 6 = 21 bits
        if (codePoint <= 0x7F)
        {
            return [new("0", Bin(codePoint, 7))];
        }
        if (codePoint <= 0x7FF)
        {
            return [
                new("110", Bin(codePoint >> 6, 5)),
                new("10",  Bin(codePoint & 0x3F, 6)),
            ];
        }
        if (codePoint <= 0xFFFF)
        {
            return [
                new("1110", Bin(codePoint >> 12, 4)),
                new("10",   Bin((codePoint >> 6) & 0x3F, 6)),
                new("10",   Bin(codePoint & 0x3F, 6)),
            ];
        }
        return [
            new("11110", Bin(codePoint >> 18, 3)),
            new("10",    Bin((codePoint >> 12) & 0x3F, 6)),
            new("10",    Bin((codePoint >> 6) & 0x3F, 6)),
            new("10",    Bin(codePoint & 0x3F, 6)),
        ];
    }

    private static string Bin(int value, int width) =>
        Convert.ToString(value, 2).PadLeft(width, '0');
}
