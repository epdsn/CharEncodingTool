using System.Text;

namespace CharEncodingTool.Core.Services;

public static class ByteFormatter
{
    public static string ToHexSpaced(ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes) switch
    {
        var s when s.Length == 0 => string.Empty,
        var s => string.Join(' ', Enumerable.Range(0, s.Length / 2).Select(i => s.Substring(i * 2, 2))),
    };

    public static string ToHexCompact(ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes);

    public static string ToBase64(ReadOnlySpan<byte> bytes) => Convert.ToBase64String(bytes);

    public static string ToPercentEncoded(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return string.Empty;
        var sb = new StringBuilder(bytes.Length * 3);
        foreach (var b in bytes)
        {
            sb.Append('%').Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    public static byte[] ParseHex(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return [];
        // Accept "48 65 6C", "0x48,0x65,0x6C", "48-65-6C", "48656C", "\x48\x65"
        var cleaned = new StringBuilder(input.Length);
        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];
            // Prefix checks must run before IsHexDigit since '0' is itself a hex digit.
            if ((c is '0' or '\\') && i + 1 < input.Length && (input[i + 1] is 'x' or 'X'))
            {
                i += 2;
                continue;
            }
            if (IsHexDigit(c))
            {
                cleaned.Append(c);
            }
            // any other char (space, comma, dash, %, etc.) is a separator
            i++;
        }
        if (cleaned.Length % 2 != 0)
            throw new FormatException($"Hex input has an odd number of digits ({cleaned.Length}).");

        return Convert.FromHexString(cleaned.ToString());
    }

    public static byte[] ParseBase64(string input) =>
        Convert.FromBase64String((input ?? string.Empty).Trim());

    private static bool IsHexDigit(char c) =>
        c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');
}
