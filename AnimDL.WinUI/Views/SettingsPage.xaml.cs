using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Web;
using AnimDL.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace AnimDL.WinUI.Views;

public class SettingsPageBase : ReactivePage<SettingsViewModel> { }
public sealed partial class SettingsPage : SettingsPageBase
{
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        this.Bind(ViewModel, vm => vm.IsPaneOpen, view => view.SplitView.IsPaneOpen);
        this.Bind(ViewModel, vm => vm.AuthUrl, view => view.WebView.Source);
        this.BindCommand(ViewModel, vm => vm.AuthenticateCommand, view => view.AuthenticateButton);
        this.WhenActivated(d =>
        {
            WebView.Events().NavigationCompleted
                            .Select(x => x.sender.Source.OriginalString)
                            .Where(x => x.Contains("github"))
                            .InvokeCommand(ViewModel.SaveAuthTokenCommand)
                            .DisposeWith(d);
        });
    }
}
