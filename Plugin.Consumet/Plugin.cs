using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("A0FB36D9-8C8A-401B-AE52-01F2F844E7A8");

    public ProviderOptions GetOptions()
    {
        return new ProviderOptions()
            .AddSelectableOption(nameof(Config.Provider), "Provider", Config.Provider, new[] { "zoro", "crunchyroll" })
            .AddSelectableOption(nameof(Config.CrunchyrollStreamType), "Stream Type", Config.CrunchyrollStreamType, new[] { "subbed1", "English Dub1" })
            .AddSelectableOption(nameof(Config.CrunchyrollMediaType), "Media Type", Config.CrunchyrollMediaType, new[] {"series", "movie"});
    }

    public void SetOptions(ProviderOptions parameters)
    {
        Config.Provider = parameters.GetString(nameof(Config.Provider), Config.Provider);
        Config.CrunchyrollStreamType = parameters.GetString(nameof(Config.CrunchyrollStreamType), Config.CrunchyrollStreamType);
        Config.CrunchyrollMediaType = parameters.GetString(nameof(Config.CrunchyrollMediaType), Config.CrunchyrollMediaType);
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Consumet", "consumet", Id);
    }
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new ConsumetStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new ConsumetCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new ConsumetAiredEpisodesProvider(HttpHelper.Client);
}

public static class Config
{
    public static string Provider { get; set; } = "zoro";
    public static string CrunchyrollStreamType { get; set; } = "subbed1";
    public static string CrunchyrollMediaType { get; set; } = "series";
}