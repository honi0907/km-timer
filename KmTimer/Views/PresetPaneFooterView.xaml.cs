using KmTimer.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KmTimer.Views;

public sealed partial class PresetPaneFooterView : UserControl
{
    public MainViewModel ViewModel => App.MainViewModel;

    public PresetPaneFooterView()
    {
        InitializeComponent();
    }
}
