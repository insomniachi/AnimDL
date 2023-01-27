using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Yugen;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("C8A2E8A4-A137-4D24-8788-69923BCF9D5C");

    public ProviderOptions GetOptions()
    {
        return new ProviderOptions()
            .AddOption(nameof(Config.BaseUrl), "Url", Config.BaseUrl)
            .AddSelectableOption(nameof(Config.StreamType), "Stream Type", Config.StreamType, new[] {"sub", "dub"});
    }

    public void SetOptions(ProviderOptions parameters)
    {
        Config.BaseUrl = parameters.GetString(nameof(Config.BaseUrl), Config.BaseUrl);
        Config.StreamType = parameters.GetString(nameof(Config.StreamType), Config.StreamType);
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Yugen Anime", "yugen", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://yugen.to/";
    public static string StreamType { get; set; } = "sub";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new YugenAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new YugenAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new YugenAnimeAiredEpisodesProvider();
}