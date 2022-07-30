using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.Services;
using Microsoft.UI.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class SettingsViewModel : ReactiveObject
{

    [Reactive] public ElementTheme ElementTheme { get; set; }
    public List<ElementTheme> Themes { get; set; } = Enum.GetValues<ElementTheme>().Cast<ElementTheme>().ToList();
    public ICommand AuthenticateCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, 
                             ILocalSettingsService localSettingsService,
                             IViewService viewService)
    {
        ElementTheme = themeSelectorService.Theme;
        AuthenticateCommand = ReactiveCommand.Create(async () => await viewService.AuthenticateMal());

        this.ObservableForProperty(x => x.ElementTheme, x => x)
            .Subscribe(themeSelectorService.SetTheme);
    }

    public static string ElementThemeToString(ElementTheme theme) => theme.ToString();
    
}
