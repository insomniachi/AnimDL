using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core;
using AnimDL.Downloading;
using AnimDL.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Xabe.FFmpeg;

namespace AnimDL;

public class Program
{

    private static readonly IHost _host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.AddAnimDL();
        services.AddMediaPlayers();
        services.AddSingleton<IDownloader, Downloader>();
    })
    .ConfigureAppConfiguration(config => 
    {
        var userProfile = Environment.ExpandEnvironmentVariables("%userprofile%");
        if(userProfile != "%userprofile%")
        {
            var userProfileConfig = Path.Combine(userProfile, ".animdl", "appsettings.json");
            config.AddJsonFile(userProfileConfig, true);
        }
    })
#if DEBUG
    .UseEnvironment("development")
#endif
    .Build();

    static async Task<int> Main(string[] args)
    {
        Initialize(_host.Services.GetRequiredService<IConfiguration>());

        var rootCommand = new RootCommand();
        rootCommand.AddCommand(GrabCommand.Create());
        rootCommand.AddCommand(SearchCommand.Create());
        rootCommand.AddCommand(StreamCommand.Create());
        rootCommand.AddCommand(DownloadCommand.Create());

        return await rootCommand.InvokeAsync(args);
    }

    static void Initialize(IConfiguration config)
    {
        DefaultUrl.AnimixPlay = config["ProviderUrls:AnimixPlay"] ?? DefaultUrl.AnimixPlay;
        DefaultUrl.AnimePahe = config["ProviderUrls:AnimePahe"] ?? DefaultUrl.AnimePahe;
        DefaultUrl.GogoAnime = config["ProviderUrls:GogoAnime"] ?? DefaultUrl.GogoAnime;
        DefaultUrl.Tenshi = config["ProviderUrls:Tenshi"] ?? DefaultUrl.Tenshi;
        DefaultUrl.Zoro = config["ProviderUrls:Zoro"] ?? DefaultUrl.Zoro;

        AppOptions.ProviderType.SetDefaultValue(config.GetValue<ProviderType>("DefaultProvider"));
        if (config.GetValue<bool>("UseRichPresense"))
        {
            DiscordRpc.Initialize();
        }

        FFmpeg.SetExecutablesPath(config.GetValue<string>("FFmpegFolder"));
    }

    public static T Resolve<T>() where T : notnull => _host.Services.GetRequiredService<T>();
}