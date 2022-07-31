using System;
using System.Reactive.Linq;
using System.Web;
using AnimDL.WinUI.Contracts.Services;
using MalApi;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.Dialogs.ViewModels;

public class AuthenticateMyAnimeListViewModel : ReactiveObject
{
    public AuthenticateMyAnimeListViewModel(IConfiguration configuration,
                                            ILocalSettingsService localSettingsService)
    {
        var clientId = configuration["ClientId"];
        AuthUrl = MalAuthHelper.GetAuthUrl(clientId);

        this.ObservableForProperty(x => x.AuthUrl, x => x)
            .Where(x => x.Contains("code"))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async x =>
            {
                IsLoading = true;
                IsAuthenticated = true;
                var code = HttpUtility.ParseQueryString(x)[0];
                var token = await MalAuthHelper.DoAuth(clientId, code);
                localSettingsService.SaveSetting(token, "MalToken");
                IsLoading = false;
            });
    }

    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string AuthUrl { get; set; }
    [Reactive] public bool IsAuthenticated { get; set; }
}
