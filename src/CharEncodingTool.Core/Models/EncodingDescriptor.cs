using System.Text;

namespace CharEncodingTool.Core.Models;

public sealed record EncodingDescriptor(
    string Id,
    string DisplayName,
    Encoding Encoding,
    string Notes);
