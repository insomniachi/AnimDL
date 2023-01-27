using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.AnimePahe;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("CAF084DB-CE59-457E-93BD-5B0EF169C424");

    public ProviderOptions GetOptions()
    {
        return new ProviderOptions().AddOption(nameof(Config.BaseUrl), "Url", Config.BaseUrl);
    }

    public void SetOptions(ProviderOptions parameters)
    {
        Config.BaseUrl = parameters.GetString(nameof(Config.BaseUrl), Config.BaseUrl);
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Anime Pahe", "animepahe", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://animepahe.com/";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new AnimePaheStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AnimePaheCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AnimePaheAiredEpisodesProvider(HttpHelper.Client);
}
