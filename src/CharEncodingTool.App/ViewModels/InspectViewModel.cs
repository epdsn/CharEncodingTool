using System.Collections.ObjectModel;
using CharEncodingTool.Core.Models;
using CharEncodingTool.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CharEncodingTool.App.ViewModels;

public partial class InspectViewModel : ObservableObject
{
    public ObservableCollection<EncodingDescriptor> Encodings { get; } =
        new(EncodingCatalog.All);

    [ObservableProperty]
    public partial EncodingDescriptor SelectedEncoding { get; set; } = EncodingCatalog.GetById("utf8");

    [ObservableProperty]
    public partial ByteInputFormat BytesInputFormat { get; set; } = ByteInputFormat.Hex;

    [ObservableProperty]
    public partial string BytesInput { get; set; } = "48 65 6C C3 28 6F";

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsValid { get; set; }

    [ObservableProperty]
    public partial bool HasResult { get; set; }

    [ObservableProperty]
    public partial string Decoded { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PreErrorHex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorHex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PostErrorHex { get; set; } = string.Empty;

    [RelayCommand]
    private void Inspect()
    {
        try
        {
            HasResult = false;
            byte[] bytes = BytesInputFormat switch
            {
                ByteInputFormat.Hex => ByteFormatter.ParseHex(BytesInput ?? string.Empty),
                ByteInputFormat.Base64 => ByteFormatter.ParseBase64(BytesInput ?? string.Empty),
                _ => [],
            };

            var result = ByteValidator.Validate(bytes, SelectedEncoding);
            ApplyResult(bytes, result);
            HasResult = true;
        }
        catch (Exception ex)
        {
            IsValid = false;
            StatusText = "Could not parse input";
            ErrorMessage = ex.Message;
            Decoded = string.Empty;
            PreErrorHex = ErrorHex = PostErrorHex = string.Empty;
            HasResult = true;
        }
    }

    [RelayCommand]
    private void LoadInvalidUtf8Example()
    {
        SelectedEncoding = EncodingCatalog.GetById("utf8");
        BytesInputFormat = ByteInputFormat.Hex;
        BytesInput = "48 65 6C C3 28 6F";
        Inspect();
    }

    [RelayCommand]
    private void LoadModifiedUtf8NullExample()
    {
        SelectedEncoding = EncodingCatalog.GetById("utf8");
        BytesInputFormat = ByteInputFormat.Hex;
        BytesInput = "C0 80";
        Inspect();
    }

    [RelayCommand]
    private void LoadLoneSurrogateExample()
    {
        SelectedEncoding = EncodingCatalog.GetById("utf16-le");
        BytesInputFormat = ByteInputFormat.Hex;
        BytesInput = "3C D8";
        Inspect();
    }

    private void ApplyResult(byte[] bytes, ValidationResult result)
    {
        IsValid = result.IsValid;
        if (result.IsValid)
        {
            StatusText = $"✓ Valid {SelectedEncoding.DisplayName}";
            Decoded = result.Decoded;
            ErrorMessage = string.Empty;
            PreErrorHex = string.Join(' ', bytes.Select(b => b.ToString("X2")));
            ErrorHex = string.Empty;
            PostErrorHex = string.Empty;
        }
        else
        {
            StatusText = $"✗ Invalid {SelectedEncoding.DisplayName}";
            Decoded = string.Empty;
            ErrorMessage = result.ErrorMessage;
            int idx = Math.Clamp(result.ErrorByteIndex, 0, bytes.Length);
            PreErrorHex  = idx > 0           ? string.Join(' ', bytes.Take(idx).Select(b => b.ToString("X2"))) + " " : string.Empty;
            ErrorHex     = idx < bytes.Length ? bytes[idx].ToString("X2") : string.Empty;
            PostErrorHex = idx + 1 < bytes.Length ? " " + string.Join(' ', bytes.Skip(idx + 1).Select(b => b.ToString("X2"))) : string.Empty;
        }
    }
}
