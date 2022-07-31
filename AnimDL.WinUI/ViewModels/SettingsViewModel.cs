using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.Services;
using Microsoft.UI.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using AnimDL.WinUI.Contracts;
using AnimDL.Api;

namespace AnimDL.WinUI.ViewModels;

public class SettingsViewModel : ReactiveObject, ISettings
{

    [Reactive] public ElementTheme ElementTheme { get; set; }
    [Reactive] public bool PreferSubs { get; set; }
    [Reactive] public ProviderType DefaultProviderType { get; set; }
    public List<ElementTheme> Themes { get; set; } = Enum.GetValues<ElementTheme>().Cast<ElementTheme>().ToList();
    public List<ProviderType> ProviderTypes { get; set; } = Enum.GetValues<ProviderType>().Cast<ProviderType>().ToList();
    
    public ICommand AuthenticateCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, 
                             ILocalSettingsService localSettingsService,
                             IViewService viewService)
    {
        ElementTheme = themeSelectorService.Theme;
        PreferSubs = localSettingsService.ReadSetting("PreferSubs", true);
        DefaultProviderType = localSettingsService.ReadSetting("DefaultProviderType", ProviderType.AnimixPlay);
        
        AuthenticateCommand = ReactiveCommand.Create(async () => await viewService.AuthenticateMal());

        this.ObservableForProperty(x => x.ElementTheme, x => x)
            .Subscribe(themeSelectorService.SetTheme);
        this.ObservableForProperty(x => x.PreferSubs, x => x)
            .Subscribe(x => localSettingsService.SaveSetting(PreferSubs));
        this.ObservableForProperty(x => x.DefaultProviderType, x => x)
            .Subscribe(x => localSettingsService.SaveSetting(DefaultProviderType));
    }

    public static string ElementThemeToString(ElementTheme theme) => theme.ToString();
    
}
