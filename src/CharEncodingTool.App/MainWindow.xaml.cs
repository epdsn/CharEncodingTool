using System.Windows;
using CharEncodingTool.App.ViewModels;

namespace CharEncodingTool.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
