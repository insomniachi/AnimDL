using AnimDL.Core.Api;
using AnimDL.Core.Helpers;

namespace Plugin.AllAnime;


public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("54F3E0C3-64A2-4C81-9FBD-F6E8ABF2AA08");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("AllAnime", "allanime", Id);
    }
}

public class Provider : IProvider
{
    public ProviderType ProviderType => ProviderType.AllAnime;

    public IStreamProvider StreamProvider { get; } = new AllAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new AllAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new AllAnimeAiredEpisodesProvider();
}
