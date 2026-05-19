namespace CharEncodingTool.Core.Services;

// A from-scratch UTF-8 encoder. Exists to make the encoding rules concrete; the test suite
// asserts byte-for-byte parity with Encoding.UTF8.GetBytes so the spec is encoded in code,
// not just prose.
//
// UTF-8 layout, by code-point range:
//   U+0000  – U+007F     1 byte    0xxxxxxx
//   U+0080  – U+07FF     2 bytes   110xxxxx 10xxxxxx
//   U+0800  – U+FFFF     3 bytes   1110xxxx 10xxxxxx 10xxxxxx
//   U+10000 – U+10FFFF   4 bytes   11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
//
// 'x' positions hold the code-point bits, packed big-endian (highest bits first).
// Continuation bytes always start 10 — that's how decoders re-synchronise after a bad byte.
public static class ManualUtf8Encoder
{
    // .NET emits U+FFFD ("REPLACEMENT CHARACTER", encoded as EF BF BD) for any lone surrogate
    // in the input string. We match that to keep byte-for-byte parity with Encoding.UTF8.
    private const int ReplacementCodePoint = 0xFFFD;

    public static byte[] Encode(string input)
    {
        if (string.IsNullOrEmpty(input)) return [];

        var output = new List<byte>(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            int codePoint = ReadCodePoint(input, ref i);
            EncodeCodePoint(codePoint, output);
        }
        return output.ToArray();
    }

    private static int ReadCodePoint(string input, ref int i)
    {
        char c = input[i];
        if (char.IsHighSurrogate(c))
        {
            if (i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
            {
                int cp = char.ConvertToUtf32(c, input[i + 1]);
                i++; // consume the paired low surrogate too
                return cp;
            }
            return ReplacementCodePoint;
        }
        if (char.IsLowSurrogate(c))
        {
            return ReplacementCodePoint;
        }
        return c;
    }

    private static void EncodeCodePoint(int cp, List<byte> output)
    {
        if (cp <= 0x7F)
        {
            // 0xxxxxxx — identical to ASCII for U+0000–U+007F.
            output.Add((byte)cp);
        }
        else if (cp <= 0x7FF)
        {
            // 110xxxxx 10xxxxxx — 11 code-point bits split 5+6.
            output.Add((byte)(0b1100_0000 | (cp >> 6)));
            output.Add((byte)(0b1000_0000 | (cp & 0b0011_1111)));
        }
        else if (cp <= 0xFFFF)
        {
            // 1110xxxx 10xxxxxx 10xxxxxx — 16 code-point bits split 4+6+6.
            output.Add((byte)(0b1110_0000 | (cp >> 12)));
            output.Add((byte)(0b1000_0000 | ((cp >> 6) & 0b0011_1111)));
            output.Add((byte)(0b1000_0000 | (cp & 0b0011_1111)));
        }
        else
        {
            // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx — 21 code-point bits split 3+6+6+6.
            // Max valid code point is U+10FFFF (21 bits).
            output.Add((byte)(0b1111_0000 | (cp >> 18)));
            output.Add((byte)(0b1000_0000 | ((cp >> 12) & 0b0011_1111)));
            output.Add((byte)(0b1000_0000 | ((cp >> 6) & 0b0011_1111)));
            output.Add((byte)(0b1000_0000 | (cp & 0b0011_1111)));
        }
    }
}
