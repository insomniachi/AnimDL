using AnimDL.Core.Api;
using AnimDL.Core.Helpers;

namespace Plugin.Yugen;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("C8A2E8A4-A137-4D24-8788-69923BCF9D5C");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Yugen Anime", "yugen", Id);
    }
}

public class Provider : IProvider
{
    public ProviderType ProviderType => ProviderType.Yugen;

    public IStreamProvider StreamProvider { get; } = new YugenAnimeStreamProvider(HttpHelper.Client);

    public ICatalog Catalog { get; } = new YugenAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new YugenAnimeAiredEpisodesProvider();
}