using System.Text;
using CharEncodingTool.Core.Models;

namespace CharEncodingTool.Core.Services;

public static class ByteValidator
{
    public static ValidationResult Validate(byte[] bytes, EncodingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        bytes ??= [];

        // Encoding instances are shared and immutable in their fallback settings — Clone and
        // swap in an exception fallback so invalid byte sequences raise instead of being replaced.
        var strict = (Encoding)descriptor.Encoding.Clone();
        strict.DecoderFallback = DecoderFallback.ExceptionFallback;

        int offset = 0;
        var preamble = descriptor.Encoding.GetPreamble();
        if (preamble.Length > 0 && bytes.Length >= preamble.Length
            && bytes.AsSpan(0, preamble.Length).SequenceEqual(preamble))
        {
            offset = preamble.Length;
        }

        try
        {
            string decoded = strict.GetString(bytes, offset, bytes.Length - offset);
            return ValidationResult.Success(decoded);
        }
        catch (DecoderFallbackException ex)
        {
            // ex.Index is the offset within the call's input where the bad sequence began.
            int absoluteIndex = offset + ex.Index;
            var bytesUnknown = ex.BytesUnknown ?? [];
            string hexBytes = bytesUnknown.Length == 0
                ? "(unknown)"
                : string.Join(' ', bytesUnknown.Select(b => b.ToString("X2")));
            string message = $"Invalid byte sequence at byte index {absoluteIndex}: {hexBytes}. {ex.Message}";
            return ValidationResult.Failure(absoluteIndex, message);
        }
    }
}
