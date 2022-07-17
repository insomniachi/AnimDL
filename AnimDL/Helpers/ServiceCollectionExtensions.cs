using AnimDL.Api;
using AnimDL.Media;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediaPlayers(this IServiceCollection services)
    {
        services.AddSingleton<IMediaPlayerFactory, MediaPlayerFactory>();
        services.AddTransient<IMediaPlayer, VlcMediaPlayer>();

        return services;
    }
}
