using AnimDL.Api;
using AnimDL.Media;
using AnimDL.ViewModels;
using AnimDL.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AnimDL.Helpers;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        //views
        services.AddTransient<SearchDialog>();
        services.AddTransient<SearchResultDialog>();
        services.AddTransient<StreamsDialog>();

        //viewmodels
        services.AddTransient<SearchViewModel>();
        services.AddTransient<SearchResultViewModel>();
        services.AddTransient<StreamsViewModel>();

        return services;
    }

    public static IServiceCollection AddMediaPlayers(this IServiceCollection services)
    {
        services.AddSingleton<IMediaPlayerFactory, MediaPlayerFactory>();
        services.AddTransient<IMediaPlayer, VlcMediaPlayer>();

        return services;
    }
}
