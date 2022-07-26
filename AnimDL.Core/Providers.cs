using AnimDL.Api;
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

    public BaseProvider(IStreamProvider provider, ICatalog catalog)
    {
        StreamProvider = provider;
        Catalog = catalog;
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
    public GogoAnimeProvider(GogoAnimeStreamProvider provider, GogoAnimeCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.GogoAnime;
}

internal class ZoroProvider : BaseProvider
{
    public ZoroProvider(ZoroStreamProvider provider, ZoroCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Zoro;
}

internal class YugenAnimeProvider : BaseProvider
{
    public YugenAnimeProvider(YugenAnimeStreamProvider provider, YugenAnimeCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Yugen;
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnimDL(this IServiceCollection services)
    {
        services.AddSingleton<HttpClient>();
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

