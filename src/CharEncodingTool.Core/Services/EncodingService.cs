using CharEncodingTool.Core.Models;

namespace CharEncodingTool.Core.Services;

public static class EncodingService
{
    public static EncodingResult Encode(string input, EncodingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        input ??= string.Empty;

        byte[] bytes = descriptor.Encoding.GetBytes(input);
        // The .NET Encoding API for UTF8/UTF16/UTF32 only emits the BOM via GetPreamble — we have to
        // prepend it ourselves to match the configured byteOrderMark flag for byte-level callers.
        byte[] preamble = descriptor.Encoding.GetPreamble();
        if (preamble.Length > 0)
        {
            var combined = new byte[preamble.Length + bytes.Length];
            Buffer.BlockCopy(preamble, 0, combined, 0, preamble.Length);
            Buffer.BlockCopy(bytes, 0, combined, preamble.Length, bytes.Length);
            bytes = combined;
        }

        return new EncodingResult(
            Descriptor: descriptor,
            Bytes: bytes,
            HexSpaced: ByteFormatter.ToHexSpaced(bytes),
            HexCompact: ByteFormatter.ToHexCompact(bytes),
            Base64: ByteFormatter.ToBase64(bytes),
            PercentEncoded: ByteFormatter.ToPercentEncoded(bytes),
            ByteCount: bytes.Length);
    }

    public static IReadOnlyList<EncodingResult> EncodeAll(string input) =>
        EncodingCatalog.All.Select(d => Encode(input, d)).ToList();

    public static string Decode(byte[] bytes, EncodingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        bytes ??= [];

        // If the buffer starts with the descriptor's preamble, strip it before decoding so the
        // resulting string doesn't begin with a U+FEFF BOM character.
        var preamble = descriptor.Encoding.GetPreamble();
        int offset = 0;
        if (preamble.Length > 0 && bytes.Length >= preamble.Length
            && bytes.AsSpan(0, preamble.Length).SequenceEqual(preamble))
        {
            offset = preamble.Length;
        }
        return descriptor.Encoding.GetString(bytes, offset, bytes.Length - offset);
    }
}
