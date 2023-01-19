using AnimDL.Core.Api;
using AnimDL.Core.Helpers;

namespace Plugin.AnimePahe;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("CAF084DB-CE59-457E-93BD-5B0EF169C424");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Anime Pahe", "animepahe", Id);
    }
}

public class Provider : IProvider
{
    public ProviderType ProviderType => ProviderType.AnimePahe;

    public IStreamProvider StreamProvider { get; } = new AnimePaheStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AnimePaheCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AnimePaheAiredEpisodesProvider(HttpHelper.Client);
}
