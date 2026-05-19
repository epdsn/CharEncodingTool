using CommunityToolkit.Mvvm.ComponentModel;

namespace CharEncodingTool.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ComparisonViewModel Comparison { get; } = new();
    public ConverterViewModel Converter { get; } = new();
}
