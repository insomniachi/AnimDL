using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using CommonConfig = AnimDL.Core.Config;

namespace Plugin.AllAnime;


public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("54F3E0C3-64A2-4C81-9FBD-F6E8ABF2AA08");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [CommonConfig.BaseUrl] = Config.BaseUrl,
            [Config.Keys.StreamType] = Config.StreamType
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(CommonConfig.BaseUrl, out string url))
        {
            Config.BaseUrl = url;
        }
        if (parameters.TryGetValue(Config.Keys.StreamType, out string type))
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

    public static class Keys
    {
        public static string StreamType { get; set; } = "StreamType";
    }
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new AllAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AllAnimeCatalog();

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AllAnimeAiredEpisodesProvider();
}
