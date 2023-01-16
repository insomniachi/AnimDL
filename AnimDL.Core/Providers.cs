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
    public AnimePaheProvider(AnimePaheStreamProvider provider, AnimePaheCatalog catalog, AnimePaheAiredEpisodesProvider episodesProvider) : base(provider, catalog, episodesProvider) { }
    public override ProviderType ProviderType => ProviderType.AnimePahe;
}

[Obsolete("Rebranded to Marin")]
internal class TenshiMoeProvider : BaseProvider
{
    public TenshiMoeProvider(TenshiMoeStreamProvider provider, TenshiCatalog catalog, TenshiAiredEpisodesProvider episodesProvider) : base(provider, catalog, episodesProvider) { }
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

internal class AllAnimeProvider : BaseProvider
{
    public AllAnimeProvider(AllAnimeStreamProvider provider, AllAnimeCatalog catalog, AllAnimeAiredEpisodesProvider episodesProvider) : base(provider, catalog, episodesProvider) { }
    public override ProviderType ProviderType => ProviderType.AllAnime;
}

internal class MarinMoeProvider : BaseProvider
{
    public MarinMoeProvider(MarinStreamProvider provider, MarinCatalog catalog, MarinAiredEpisodesProvider airedEpisodesProvider) : base(provider, catalog, airedEpisodesProvider) { }
    public override ProviderType ProviderType => ProviderType.Marin;
}

public static class ServiceCollectionExtensions
{
    public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.81 Safari/537.36 Edg/104.0.1293.54";

#pragma warning disable CS0618 // Type or member is obsolete

    private static IServiceCollection AddProvider<TProvider, TCatalog, TStreamProvider, TAiredEpisodesProvider>(this IServiceCollection services)
        where TProvider : class, IProvider
        where TStreamProvider: class, IStreamProvider
        where TCatalog : class, ICatalog
        where TAiredEpisodesProvider : class, IAiredEpisodeProvider
    {
        return services.AddTransient<IProvider, TProvider>()
                       .AddTransient<TCatalog>()
                       .AddTransient<TStreamProvider>()
                       .AddTransient<TAiredEpisodesProvider>();
    }

    private static IServiceCollection AddProvider<TProvider, TCatalog, TStreamProvider>(this IServiceCollection services)
        where TProvider : class, IProvider
        where TStreamProvider : class, IStreamProvider
        where TCatalog : class, ICatalog
    {
        return services.AddTransient<IProvider, TProvider>()
                       .AddTransient<TCatalog>()
                       .AddTransient<TStreamProvider>();
    }

    public static IServiceCollection AddAnimDL(this IServiceCollection services)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
        services.AddSingleton(client);
        services.AddTransient<IProviderFactory, ProviderFactory>();

        //extractors
        services.AddTransient<GogoPlayExtractor>();
        services.AddTransient<RapidVideoExtractor>();

        //providers
        services.AddProvider<GogoAnimeProvider, GogoAnimeCatalog, GogoAnimeStreamProvider, GogoAnimeEpisodesProvider>();
        services.AddProvider<YugenAnimeProvider, YugenAnimeCatalog, YugenAnimeStreamProvider, YugenAnimeAiredEpisodesProvider>();
        services.AddProvider<AllAnimeProvider, AllAnimeCatalog, AllAnimeStreamProvider, AllAnimeAiredEpisodesProvider>();
        services.AddProvider<AnimePaheProvider, AnimePaheCatalog, AnimePaheStreamProvider, AnimePaheAiredEpisodesProvider>();
        services.AddProvider<MarinMoeProvider, MarinCatalog, MarinStreamProvider, MarinAiredEpisodesProvider>();
        services.AddProvider<TenshiMoeProvider, TenshiCatalog, TenshiMoeStreamProvider, TenshiAiredEpisodesProvider>();
        services.AddProvider<AnimixPlayProvider, AnimixPlayCatalog, AnimixPlayStreamProvider>();
        services.AddProvider<AnimeOutProvider, AnimeOutCatalog, AnimeOutStreamProvider>();
        services.AddProvider<ZoroProvider, ZoroCatalog, ZoroStreamProvider>();

        return services;
    }
}


