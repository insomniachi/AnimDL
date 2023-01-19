using AnimDL.Core.Api;
using AnimDL.Core.Helpers;

namespace Plugin.Marin;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("531446DD-41F1-45C0-952D-9EE5741F4E01");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Marin Moe", "marin", Id);
    }
}

public class Provider : IProvider
{
    public ProviderType ProviderType => ProviderType.Marin;

    public IStreamProvider StreamProvider { get; } = new MarinStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new MarinCatalog();

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new MarinAiredEpisodesProvider();

}
