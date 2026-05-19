namespace CharEncodingTool.Core.Models;

public sealed record EncodingResult(
    EncodingDescriptor Descriptor,
    byte[] Bytes,
    string HexSpaced,
    string HexCompact,
    string Base64,
    string PercentEncoded,
    int ByteCount);
