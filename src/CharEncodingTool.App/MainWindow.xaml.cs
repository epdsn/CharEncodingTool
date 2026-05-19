using System.Windows;
using System.Windows.Controls;
using CharEncodingTool.App.ViewModels;

namespace CharEncodingTool.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement el && el.Tag is string s && !string.IsNullOrEmpty(s))
        {
            try { Clipboard.SetText(s); } catch { /* clipboard can fail if another process owns it; ignore */ }
        }
    }
}
