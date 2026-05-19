namespace CharEncodingTool.Core.Models;

public sealed record ValidationResult(
    bool IsValid,
    string Decoded,
    int ErrorByteIndex,
    string ErrorMessage)
{
    public static ValidationResult Success(string decoded) =>
        new(true, decoded, -1, string.Empty);

    public static ValidationResult Failure(int byteIndex, string message) =>
        new(false, string.Empty, byteIndex, message);
}
