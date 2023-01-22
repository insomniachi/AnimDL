using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("A0FB36D9-8C8A-401B-AE52-01F2F844E7A8");

    public IParameters GetDefaultConfig()
    {
        return new Parameters
        {
            [nameof(Config.Provider)] = Config.Provider,
            [nameof(Config.CrunchyrollStreamType)] = Config.CrunchyrollStreamType,
            [nameof(Config.CrunchyrollMediaType)] = Config.CrunchyrollMediaType,
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(nameof(Config.Provider), out string provider))
        {
            Config.Provider = provider;
        }
        if (parameters.TryGetValue(nameof(Config.CrunchyrollStreamType), out string type))
        {
            Config.CrunchyrollStreamType = type;
        }
        if (parameters.TryGetValue(nameof(Config.CrunchyrollMediaType), out string mediaType))
        {
            Config.CrunchyrollMediaType = mediaType;
        }
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
    public static string CrunchyrollStreamType { get; set; } = "sub";
    public static string CrunchyrollMediaType { get; set; } = "series";
}