using AnimDL.Core.Api;
using AnimDL.Core.Helpers;

namespace Plugin.GogoAnime;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("78EF8C43-DD16-4FB6-8503-9869802196A1");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<Provider>("Gogo Anime", "gogo", Id);
    }
}

public class Provider : IProvider
{
    public ProviderType ProviderType => ProviderType.GogoAnime;

    public IStreamProvider StreamProvider { get; } = new GogoAnimeStreamProvider(new AnimDL.Core.Extractors.GogoPlayExtractor(), HttpHelper.Client);

    public ICatalog Catalog { get; } = new GogoAnimeCatalog(HttpHelper.Client);

    public IAiredEpisodeProvider AiredEpisodesProvider { get; } = new GogoAnimeEpisodesProvider();

    public string Name => "gogo";
}
