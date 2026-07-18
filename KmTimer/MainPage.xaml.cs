using KmTimer.ViewModels;
using KmTimer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KmTimer;

public sealed partial class MainPage : Page
{
    private readonly Dictionary<ControlPanelSection, UserControl> _sectionViews = new();
    private bool _isNavInitializing;

    public MainViewModel ViewModel => App.MainViewModel;

    public MainPage()
    {
        InitializeComponent();
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        _sectionViews[ControlPanelSection.Timer] = new TimerSettingsView();
        _sectionViews[ControlPanelSection.Kanpe] = new KanpeSettingsView();
        _sectionViews[ControlPanelSection.System] = new SystemSettingsView();

        _isNavInitializing = true;
        NavView.SelectedItem = NavView.MenuItems[0];
        _isNavInitializing = false;

        ShowSection(ControlPanelSection.Timer);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_isNavInitializing || args.SelectedItem is not NavigationViewItem item)
            return;

        var section = ParseSection(item.Tag as string);
        ShowSection(section);
    }

    private void ShowSection(ControlPanelSection section)
    {
        if (_sectionViews.TryGetValue(section, out var view))
            DetailPresenter.Content = view;
    }

    private static ControlPanelSection ParseSection(string? tag) => tag switch
    {
        "kanpe" => ControlPanelSection.Kanpe,
        "system" => ControlPanelSection.System,
        _ => ControlPanelSection.Timer
    };
}
