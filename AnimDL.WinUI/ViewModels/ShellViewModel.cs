using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace AnimDL.WinUI.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private object _selected;

    [ObservableProperty]
    private bool _isBackEnabled;

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; set; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;
        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
