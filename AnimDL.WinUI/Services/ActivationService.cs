using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimDL.WinUI.Activation;
using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Core.Contracts;
using AnimDL.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AnimDL.WinUI.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IPlaybackStateStorage _playbackStateStorage;
    private UIElement _shell = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, 
                             IEnumerable<IActivationHandler> activationHandlers,
                             IThemeSelectorService themeSelectorService,
                             IPlaybackStateStorage playbackStateStorage)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _playbackStateStorage = playbackStateStorage;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        App.MainWindow.Closed += MainWindow_Closed;

        // Execute tasks after activation.
        await StartupAsync();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        _playbackStateStorage.StoreState();
        App.MainWindow.Closed -= MainWindow_Closed;
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        _themeSelectorService.Initialize();
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        _themeSelectorService.SetRequestedTheme();
        await Task.CompletedTask;
    }
}
