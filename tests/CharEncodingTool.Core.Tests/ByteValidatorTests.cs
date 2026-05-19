using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class ByteValidatorTests
{
    [Fact]
    public void Validate_ValidUtf8_ReturnsSuccess()
    {
        var bytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf8"));
        Assert.True(result.IsValid);
        Assert.Equal("Hello", result.Decoded);
        Assert.Equal(-1, result.ErrorByteIndex);
    }

    [Fact]
    public void Validate_InvalidUtf8_BadContinuationByte_ReportsErrorIndex()
    {
        // C3 28: C3 starts a 2-byte sequence but 28 is not a valid continuation byte (0x80-0xBF).
        var bytes = new byte[] { 0x48, 0x65, 0x6C, 0xC3, 0x28, 0x6F };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf8"));
        Assert.False(result.IsValid);
        Assert.Equal(3, result.ErrorByteIndex);
        Assert.Contains("byte index 3", result.ErrorMessage);
    }

    [Fact]
    public void Validate_InvalidUtf8_OverlongEncoding_Fails()
    {
        // C0 80 is the "Modified UTF-8" encoding of NUL — invalid under strict UTF-8.
        var bytes = new byte[] { 0xC0, 0x80 };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf8"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_TruncatedMultiByteUtf8_Fails()
    {
        // C3 is the start of a 2-byte sequence but the continuation byte is missing.
        var bytes = new byte[] { 0x48, 0xC3 };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf8"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Utf16Le_LoneSurrogate_Fails()
    {
        // D8 3C with no following low surrogate.
        var bytes = new byte[] { 0x3C, 0xD8 };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf16-le"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Utf16Le_ValidSurrogatePair_Succeeds()
    {
        // 🎉 = U+1F389 → 3C D8 89 DF in UTF-16 LE
        var bytes = new byte[] { 0x3C, 0xD8, 0x89, 0xDF };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf16-le"));
        Assert.True(result.IsValid);
        Assert.Equal("🎉", result.Decoded);
    }

    [Fact]
    public void Validate_Utf8WithBom_StripsBomBeforeValidating()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF, 0x41 };
        var result = ByteValidator.Validate(bytes, EncodingCatalog.GetById("utf8-bom"));
        Assert.True(result.IsValid);
        Assert.Equal("A", result.Decoded);
    }

    [Fact]
    public void Validate_EmptyBytes_Succeeds()
    {
        var result = ByteValidator.Validate([], EncodingCatalog.GetById("utf8"));
        Assert.True(result.IsValid);
        Assert.Equal(string.Empty, result.Decoded);
    }
}
