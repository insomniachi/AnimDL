using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.AllAnime;


public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("54F3E0C3-64A2-4C81-9FBD-F6E8ABF2AA08");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [nameof(Config.BaseUrl)] = Config.BaseUrl,
            [nameof(Config.StreamType)] = Config.StreamType
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(nameof(Config.BaseUrl), out string url))
        {
            Config.BaseUrl = url;
        }
        if (parameters.TryGetValue(nameof(Config.StreamType), out string type))
        {
            Config.StreamType = type;
        }
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("AllAnime", "allanime", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://allanime.site/";
    public static string StreamType { get; set; } = "sub";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new AllAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AllAnimeCatalog();

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AllAnimeAiredEpisodesProvider();
}
