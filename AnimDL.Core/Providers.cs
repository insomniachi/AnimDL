using AnimDL.Api;
using AnimDL.Core.AiredEpisodesProvider;
using AnimDL.Core.Api;
using AnimDL.Core.Catalog;
using AnimDL.Core.Extractors;
using AnimDL.Core.StreamProviders;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Core;

public abstract class BaseProvider : IProvider
{
    public abstract ProviderType ProviderType { get; }
    public IStreamProvider StreamProvider { get; }
    public ICatalog Catalog { get; }
    public IAiredEpisodeProvider? AiredEpisodesProvider { get; }

    public BaseProvider(IStreamProvider provider, ICatalog catalog, IAiredEpisodeProvider? airedEpisodesProvider = null)
    {
        StreamProvider = provider;
        Catalog = catalog;
        AiredEpisodesProvider = airedEpisodesProvider;
    }
}

public class ProviderFactory : IProviderFactory
{
    private readonly IEnumerable<IProvider> _providers;

    public ProviderFactory(IEnumerable<IProvider> providers)
    {
        _providers = providers;
    }

    public IProvider GetProvider(ProviderType type) => _providers.First(x => x.ProviderType == type);
}

[Obsolete("RIP")]
internal class AnimixPlayProvider : BaseProvider
{
    public AnimixPlayProvider(AnimixPlayStreamProvider provider, AnimixPlayCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimixPlay;
}

internal class AnimePaheProvider : BaseProvider
{
    public AnimePaheProvider(AnimePaheStreamProvider provider, AnimePaheCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimePahe;
}

internal class TenshiMoeProvider : BaseProvider
{
    public TenshiMoeProvider(TenshiMoeStreamProvider provider, TenshiCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Tenshi;
}

internal class AnimeOutProvider : BaseProvider
{
    public AnimeOutProvider(AnimeOutStreamProvider provider, AnimeOutCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimeOut;
}

internal class GogoAnimeProvider : BaseProvider
{
    public GogoAnimeProvider(GogoAnimeStreamProvider provider, GogoAnimeCatalog catalog, GogoAnimeEpisodesProvider episodesProvider) : base(provider, catalog, episodesProvider) { }
    public override ProviderType ProviderType => ProviderType.GogoAnime;
}

internal class ZoroProvider : BaseProvider
{
    public ZoroProvider(ZoroStreamProvider provider, ZoroCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Zoro;
}

internal class YugenAnimeProvider : BaseProvider
{
    public YugenAnimeProvider(YugenAnimeStreamProvider provider, YugenAnimeCatalog catalog, YugenAnimeAiredEpisodesProvider episodesProvider) : base(provider, catalog, episodesProvider) { }
    public override ProviderType ProviderType => ProviderType.Yugen;
}

public static class ServiceCollectionExtensions
{
    const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.81 Safari/537.36 Edg/104.0.1293.54";

#pragma warning disable CS0618 // Type or member is obsolete
    public static IServiceCollection AddAnimDL(this IServiceCollection services)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
        services.AddSingleton(client);
        services.AddTransient<IProviderFactory, ProviderFactory>();

        // streams
        services.AddTransient<AnimixPlayStreamProvider>();
        services.AddTransient<AnimePaheStreamProvider>();
        services.AddTransient<TenshiMoeStreamProvider>();
        services.AddTransient<AnimeOutStreamProvider>();
        services.AddTransient<GogoAnimeStreamProvider>();
        services.AddTransient<ZoroStreamProvider>();
        services.AddTransient<YugenAnimeStreamProvider>();

        //catalog
        services.AddTransient<AnimixPlayCatalog>();
        services.AddTransient<AnimePaheCatalog>();
        services.AddTransient<TenshiCatalog>();
        services.AddTransient<AnimeOutCatalog>();
        services.AddTransient<GogoAnimeCatalog>();
        services.AddTransient<ZoroCatalog>();
        services.AddTransient<YugenAnimeCatalog>();

        //extractors
        services.AddTransient<GogoPlayExtractor>();
        services.AddTransient<RapidVideoExtractor>();

        //recent episodes
        services.AddTransient<GogoAnimeEpisodesProvider>();
        services.AddTransient<YugenAnimeAiredEpisodesProvider>();

        //providers
        services.AddTransient<IProvider, AnimixPlayProvider>();
        services.AddTransient<IProvider, AnimePaheProvider>();
        services.AddTransient<IProvider, TenshiMoeProvider>();
        services.AddTransient<IProvider, AnimeOutProvider>();
        services.AddTransient<IProvider, GogoAnimeProvider>();
        services.AddTransient<IProvider, ZoroProvider>();
        services.AddTransient<IProvider, YugenAnimeProvider>();

        return services;
    }
}


