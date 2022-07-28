using System;
using System.Threading.Tasks;
using AnimDL.WinUI.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;

namespace AnimDL.WinUI.Services;

public class ContentDialogService : IContentDialogService
{
    public async Task<ContentDialogResult> ShowDialog<TViewModel>(Action<ContentDialog> configure)
        where TViewModel : class
    {
        var vm = App.GetService<TViewModel>();
        return await ShowDialog(vm, configure);
    }

    public async Task<ContentDialogResult> ShowDialog<TViewModel>(TViewModel viewModel, Action<ContentDialog> configure)
        where TViewModel : class
    {
        var view = App.GetService<IViewFor<TViewModel>>();
        view.ViewModel = viewModel;
        var dialog = new ContentDialog()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            XamlRoot = App.MainWindow.Content.XamlRoot,
            DefaultButton = ContentDialogButton.Primary,
            Content = view
        };
        configure(dialog);
        return await dialog.ShowAsync();
    }
}
