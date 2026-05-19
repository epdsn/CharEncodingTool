using System.Collections.ObjectModel;
using CharEncodingTool.Core.Models;
using CharEncodingTool.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CharEncodingTool.App.ViewModels;

public partial class ComparisonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string InputText { get; set; } = "Hello, 世界! 🎉";

    public ObservableCollection<EncodingResult> Results { get; } = new();

    public ComparisonViewModel()
    {
        Refresh();
    }

    partial void OnInputTextChanged(string value) => Refresh();

    private void Refresh()
    {
        Results.Clear();
        foreach (var r in EncodingService.EncodeAll(InputText ?? string.Empty))
        {
            Results.Add(r);
        }
    }
}
