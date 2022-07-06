
using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Helpers;
using AnimDL.MediaPlayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.RegisterCommands()
                .RegisterProviders();

        services.AddSingleton<IMediaPlayer, MediaPlayer>();
    })
    .Build();


var config = host.Services.GetRequiredService<IConfiguration>();
var defaultProviderType = config.GetValue<ProviderType>("DefaultProvider");
AppOptions.ProviderType.SetDefaultValue(defaultProviderType);

var rootCommand = new RootCommand();
var configureCommand = new Command("configure", "configure options for application");
configureCommand.SetHandler(Configure);
rootCommand.AddCommand(configureCommand);
rootCommand.AddCommand(host.Services.GetRequiredService<StreamCommand>());
rootCommand.AddCommand(host.Services.GetRequiredService<GrabCommand>());
rootCommand.AddCommand(host.Services.GetRequiredService<SearchCommand>());

return await rootCommand.InvokeAsync(args);



void Configure()
{
    var p = new Process();
    p.StartInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
    {
        UseShellExecute = true
    };
    p.Start();
}