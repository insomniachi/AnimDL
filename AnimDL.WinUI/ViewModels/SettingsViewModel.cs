using System.Text.Json;
using System.Web;
using System.Windows.Input;

using AnimDL.WinUI.Contracts.Services;
using MalApi;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    private readonly ILocalSettingsService _localSettingsService;

    [Reactive]
    public ElementTheme ElementTheme { get; set; }

    [Reactive]
    public string VersionDescription { get; set; }

    [Reactive]
    public bool IsPaneOpen { get; set; }

    [Reactive]
    public string AuthUrl { get; set; }

    public string ClientId { get; }

    public ICommand SaveAuthTokenCommand { get; }
 
    public ICommand SwitchThemeCommand { get; }

    public ICommand AuthenticateCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, 
                             ILocalSettingsService localSettingsService,
                             IConfiguration configuration)
    {
        _localSettingsService = localSettingsService;
        ElementTheme = themeSelectorService.Theme;
        ClientId = configuration["ClientId"];

        SwitchThemeCommand = ReactiveCommand.Create<ElementTheme>(t => themeSelectorService.SetTheme(t));
        SaveAuthTokenCommand = ReactiveCommand.Create<string>(async code =>
        {
            IsPaneOpen = false;
            var token = await MalAuthHelper.DoAuth(ClientId, code);
            localSettingsService.SaveSetting("MalToken", token);
        });
        AuthenticateCommand = ReactiveCommand.Create(() =>
        {
            AuthUrl = MalAuthHelper.GetAuthUrl(ClientId);
            IsPaneOpen = true;
        });
    }
    
}
