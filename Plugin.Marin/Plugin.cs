using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using CommonConfig = AnimDL.Core.Config;

namespace Plugin.Marin;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("531446DD-41F1-45C0-952D-9EE5741F4E01");

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
        registrationContext.RegisterPlugin<Provider>("Marin Moe", "marin", Id);
    }
}

public static class Config
{
    public static string BaseUrl { get; set; } = "https://marin.moe/";
}

public class Provider : IProvider
{
    public IStreamProvider StreamProvider { get; } = new MarinStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new MarinCatalog();

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new MarinAiredEpisodesProvider();

}
