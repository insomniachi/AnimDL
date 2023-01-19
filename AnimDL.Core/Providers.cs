using System.Runtime.Loader;
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
    public string Name { get; } = string.Empty;

    public BaseProvider(IStreamProvider provider, ICatalog catalog, IAiredEpisodeProvider? airedEpisodesProvider = null)
    {
        StreamProvider = provider;
        Catalog = catalog;
        AiredEpisodesProvider = airedEpisodesProvider;
    }
}

public class ProviderFactory : IProviderFactory
{
    private readonly List<AssemblyLoadContext> _assemblyLoadContexts = new();
    private readonly IPluginRegistrationContext pluginContext = new PluginRegistrationContext();

    public static ProviderFactory Instance { get; } = new ProviderFactory();

    public IEnumerable<ProviderInfo> Providers => pluginContext.Providers;
    public IProvider? GetProvider(string name) => pluginContext.GetProvider(name);

    public void LoadPlugins(string folder)
    {
        foreach (var dll in Directory.GetFiles(folder, "*.dll"))
        {
            var context = new AssemblyLoadContext(dll, true);

            var assembly = context.LoadFromAssemblyPath(dll);
            var plugins = assembly.GetExportedTypes().Where(x => x.IsAssignableTo(typeof(IPlugin))).ToList();

            if (plugins.Count == 0)
            {
                context.Unload();
                continue;
            }

            foreach (var type in plugins)
            {
                if (!type.IsAssignableTo(typeof(IPlugin)) || !(type.FullName is { } pluginType))
                {
                    continue;
                }

                if (assembly.CreateInstance(pluginType) is not IPlugin plugIn)
                {
                    continue;
                }

                plugIn.RegisterProviders(pluginContext);

            }
            _assemblyLoadContexts.Add(context);
        }
    }

    public void LoadPlugin<TPlugin>()
        where TPlugin : IPlugin, new()
    {
        var plugin = new TPlugin();
        plugin.RegisterProviders(pluginContext);
    }

    public void UnloadPlugin(string name)
    {
        if (pluginContext.UnloadPlugin(name) is not { } plugin)
        {
            return;
        }

        if (AssemblyLoadContext.GetLoadContext(plugin.Provider.Value.GetType().Assembly) is not { } loadContext)
        {
            return;
        }

        _assemblyLoadContexts.Remove(loadContext);
        loadContext.Unload();
    }
}

[Obsolete("RIP")]
internal class AnimixPlayProvider : BaseProvider
{
    public AnimixPlayProvider(AnimixPlayStreamProvider provider, AnimixPlayCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimixPlay;
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

internal class ZoroProvider : BaseProvider
{
    public ZoroProvider(ZoroStreamProvider provider, ZoroCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Zoro;
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
        services.AddProvider<TenshiMoeProvider, TenshiCatalog, TenshiMoeStreamProvider, TenshiAiredEpisodesProvider>();
        services.AddProvider<AnimixPlayProvider, AnimixPlayCatalog, AnimixPlayStreamProvider>();
        services.AddProvider<AnimeOutProvider, AnimeOutCatalog, AnimeOutStreamProvider>();
        services.AddProvider<ZoroProvider, ZoroCatalog, ZoroStreamProvider>();

        return services;
    }
}

