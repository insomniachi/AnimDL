using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using CommonConfig = AnimDL.Core.Config;

namespace Plugin.AnimePahe;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("CAF084DB-CE59-457E-93BD-5B0EF169C424");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [CommonConfig.BaseUrl] = Config.BaseUrl
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(CommonConfig.BaseUrl, out string url))
        {
            Config.BaseUrl = url;
        }
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
