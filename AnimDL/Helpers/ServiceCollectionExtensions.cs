using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core.Catalog;
using AnimDL.Core.StreamProviders;
using AnimDL.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterProviders(this IServiceCollection services)
    {
        services.AddTransient<IProviderFactory, ProviderFactory>();

        // streams
        services.AddTransient<AnimixPlayStreamProvider>();
        services.AddTransient<AnimePaheStreamProvider>();
        services.AddTransient<TenshiMoeStreamProvider>();

        //catalog
        services.AddTransient<AnimixPlayCatalog>();
        services.AddTransient<AnimePaheCatalog>();
        services.AddTransient<TenshiMoeCatalog>();

        //providers
        services.AddTransient<IProvider, AnimixPlayProvider>();
        services.AddTransient<IProvider, AnimePaheProvider>();
        services.AddTransient<IProvider, TenshiMoeProvider>();

        return services;
    }

    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        services.AddTransient<StreamCommand>();
        services.AddTransient<GrabCommand>();
        return services;
    }
}
