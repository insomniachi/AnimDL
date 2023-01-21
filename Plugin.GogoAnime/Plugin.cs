using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using CommonKeys = AnimDL.Core.Config;

namespace Plugin.GogoAnime;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("78EF8C43-DD16-4FB6-8503-9869802196A1");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [CommonKeys.BaseUrl] = Config.BaseUrl
        };
    }

    public void Initialize(IParameters parameters)
    {
        if (parameters.TryGetValue(CommonKeys.BaseUrl, out string url))
        {
            Config.BaseUrl = url;
        }
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Gogo Anime", "gogo", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://www1.gogoanime.bid/";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new GogoAnimeStreamProvider(new AnimDL.Core.Extractors.GogoPlayExtractor(), HttpHelper.Client);

    public ICatalog Catalog { get; } = new GogoAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new GogoAnimeEpisodesProvider();
}
