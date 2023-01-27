using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Marin;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("531446DD-41F1-45C0-952D-9EE5741F4E01");

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
