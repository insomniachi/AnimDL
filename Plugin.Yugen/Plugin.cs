using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using CommonKeys = AnimDL.Core.Config;

namespace Plugin.Yugen;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("C8A2E8A4-A137-4D24-8788-69923BCF9D5C");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [CommonKeys.BaseUrl] = Config.BaseUrl
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(CommonKeys.BaseUrl, out string url))
        {
            Config.BaseUrl = url;
        }
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Yugen Anime", "yugen", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://yugen.to/";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new YugenAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new YugenAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new YugenAnimeAiredEpisodesProvider();
}