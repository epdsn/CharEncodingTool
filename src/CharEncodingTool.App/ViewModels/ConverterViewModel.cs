using System.Collections.ObjectModel;
using CharEncodingTool.Core.Models;
using CharEncodingTool.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CharEncodingTool.App.ViewModels;

public enum ByteInputFormat { Hex, Base64 }

public partial class ConverterViewModel : ObservableObject
{
    public ObservableCollection<EncodingDescriptor> Encodings { get; } =
        new(EncodingCatalog.All);

    [ObservableProperty]
    public partial EncodingDescriptor SelectedEncoding { get; set; } = EncodingCatalog.GetById("utf8");

    [ObservableProperty]
    public partial string StringInput { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BytesOutput { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BytesInput { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StringOutput { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ByteInputFormat BytesInputFormat { get; set; } = ByteInputFormat.Hex;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [RelayCommand]
    private void EncodeStringToBytes()
    {
        try
        {
            ErrorMessage = string.Empty;
            var result = EncodingService.Encode(StringInput ?? string.Empty, SelectedEncoding);
            BytesOutput = result.HexSpaced;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            BytesOutput = string.Empty;
        }
    }

    [RelayCommand]
    private void DecodeBytesToString()
    {
        try
        {
            ErrorMessage = string.Empty;
            byte[] bytes = BytesInputFormat switch
            {
                ByteInputFormat.Hex => ByteFormatter.ParseHex(BytesInput ?? string.Empty),
                ByteInputFormat.Base64 => ByteFormatter.ParseBase64(BytesInput ?? string.Empty),
                _ => [],
            };
            StringOutput = EncodingService.Decode(bytes, SelectedEncoding);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StringOutput = string.Empty;
        }
    }
}
