using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.GogoAnime;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("78EF8C43-DD16-4FB6-8503-9869802196A1");

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
