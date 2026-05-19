using System.Globalization;
using System.Text;

namespace CharEncodingTool.Core.Services;

public static class EscapeSequenceParser
{
    public static string Parse(string input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

        var sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c != '\\' || i == input.Length - 1)
            {
                sb.Append(c);
                continue;
            }

            char next = input[i + 1];
            switch (next)
            {
                case '\\': sb.Append('\\'); i++; break;
                case '\'': sb.Append('\''); i++; break;
                case '"':  sb.Append('"');  i++; break;
                case '0':  sb.Append('\0'); i++; break;
                case 'a':  sb.Append('\a'); i++; break;
                case 'b':  sb.Append('\b'); i++; break;
                case 'f':  sb.Append('\f'); i++; break;
                case 'n':  sb.Append('\n'); i++; break;
                case 'r':  sb.Append('\r'); i++; break;
                case 't':  sb.Append('\t'); i++; break;
                case 'v':  sb.Append('\v'); i++; break;
                case 'x':  i = ParseHexEscape(input, i, sb, maxDigits: 4); break;
                case 'u':  i = ParseFixedHexEscape(input, i, sb, digits: 4); break;
                case 'U':  i = ParseFixedHexEscape(input, i, sb, digits: 8); break;
                default:
                    sb.Append('\\').Append(next);
                    i++;
                    break;
            }
        }
        return sb.ToString();
    }

    // \x accepts 1–4 hex digits — variable length, terminates at first non-hex.
    private static int ParseHexEscape(string input, int backslashIndex, StringBuilder sb, int maxDigits)
    {
        int start = backslashIndex + 2;
        int end = start;
        while (end < input.Length && end - start < maxDigits && IsHexDigit(input[end]))
            end++;
        if (end == start)
        {
            sb.Append("\\x");
            return backslashIndex + 1;
        }
        int value = int.Parse(input.AsSpan(start, end - start), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        sb.Append((char)value);
        return end - 1;
    }

    // \u and \U require exactly `digits` hex digits or the escape is left untouched.
    private static int ParseFixedHexEscape(string input, int backslashIndex, StringBuilder sb, int digits)
    {
        int start = backslashIndex + 2;
        if (start + digits > input.Length || !AllHex(input, start, digits))
        {
            sb.Append(input[backslashIndex]).Append(input[backslashIndex + 1]);
            return backslashIndex + 1;
        }
        int value = int.Parse(input.AsSpan(start, digits), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        if (value <= 0xFFFF)
        {
            sb.Append((char)value);
        }
        else
        {
            // Supplementary code point — emit as surrogate pair.
            sb.Append(char.ConvertFromUtf32(value));
        }
        return start + digits - 1;
    }

    private static bool AllHex(string input, int start, int count)
    {
        for (int i = 0; i < count; i++)
            if (!IsHexDigit(input[start + i])) return false;
        return true;
    }

    private static bool IsHexDigit(char c) =>
        c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');
}
