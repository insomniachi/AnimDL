using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core.Catalog;
using AnimDL.Core.Extractors;
using AnimDL.Core.StreamProviders;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        services.AddTransient<StreamCommand>();
        services.AddTransient<GrabCommand>();
        services.AddTransient<SearchCommand>();
        return services;
    }
}
