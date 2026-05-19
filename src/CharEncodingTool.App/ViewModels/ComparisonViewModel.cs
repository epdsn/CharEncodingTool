using System.Collections.ObjectModel;
using CharEncodingTool.Core.Models;
using CharEncodingTool.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CharEncodingTool.App.ViewModels;

public partial class ComparisonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string InputText { get; set; } = "Hello, 世界! 🎉";

    [ObservableProperty]
    public partial bool InterpretEscapes { get; set; }

    [ObservableProperty]
    public partial bool ShowControlChars { get; set; } = true;

    [ObservableProperty]
    public partial string EffectivePreview { get; set; } = string.Empty;

    public ObservableCollection<EncodingResult> Results { get; } = new();
    public ObservableCollection<CodePointInfo> CodePoints { get; } = new();

    public ComparisonViewModel()
    {
        Refresh();
    }

    partial void OnInputTextChanged(string value) => Refresh();
    partial void OnInterpretEscapesChanged(bool value) => Refresh();
    partial void OnShowControlCharsChanged(bool value) => UpdatePreview(GetEffectiveInput());

    private string GetEffectiveInput() =>
        InterpretEscapes ? EscapeSequenceParser.Parse(InputText ?? string.Empty) : (InputText ?? string.Empty);

    private void UpdatePreview(string effective) =>
        EffectivePreview = ShowControlChars ? ControlCharRenderer.Render(effective) : effective;

    private void Refresh()
    {
        var effective = GetEffectiveInput();

        Results.Clear();
        foreach (var r in EncodingService.EncodeAll(effective))
        {
            Results.Add(r);
        }

        CodePoints.Clear();
        foreach (var cp in CodePointAnalyzer.Analyze(effective))
        {
            CodePoints.Add(cp);
        }

        UpdatePreview(effective);
    }
}
