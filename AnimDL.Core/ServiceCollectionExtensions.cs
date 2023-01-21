using AnimDL.Core.Api;
using AnimDL.Core.Extractors;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Core;

public static class ServiceCollectionExtensions
{
    public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.81 Safari/537.36 Edg/104.0.1293.54";

    public static IServiceCollection AddAnimDL(this IServiceCollection services)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
        services.AddSingleton(client);
        services.AddTransient<IProviderFactory, ProviderFactory>();

        //extractors
        services.AddTransient<GogoPlayExtractor>();
        services.AddTransient<RapidVideoExtractor>();

        return services;
    }
}

