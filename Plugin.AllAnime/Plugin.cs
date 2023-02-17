using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.AllAnime;


public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("54F3E0C3-64A2-4C81-9FBD-F6E8ABF2AA08");

    public ProviderOptions GetOptions()
    {
        return new ProviderOptions()
            .AddOption(nameof(Config.BaseUrl), "Url", Config.BaseUrl)
            .AddSelectableOption(nameof(Config.StreamType), "Stream Type", Config.StreamType, new[] { "sub", "dub", "raw" })
            .AddSelectableOption(nameof(Config.CountryOfOrigin), "Country of Origin", Config.CountryOfOrigin, new[] {"ALL", "JP", "CN", "KR"});
    }

    public void SetOptions(ProviderOptions parameters)
    {
        Config.BaseUrl = parameters.GetString(nameof(Config.BaseUrl), Config.BaseUrl);
        Config.StreamType = parameters.GetString(nameof(Config.StreamType), Config.StreamType);
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("AllAnime", "allanime", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://allanime.to/";
    public static string StreamType { get; set; } = "sub";
    public static string CountryOfOrigin { get; set; } = "JP";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new AllAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AllAnimeCatalog();

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AllAnimeAiredEpisodesProvider();
}
