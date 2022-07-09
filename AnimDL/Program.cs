
using AnimDL;
using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core;
using AnimDL.Helpers;
using AnimDL.MediaPlayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;
using CliCommand = System.CommandLine.Command;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.RegisterCommands();
        services.AddAnimeDl();
        services.AddSingleton<IMediaPlayer, MediaPlayer>();
    })
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var defaultProviderType = config.GetValue<ProviderType>("DefaultProvider");
AppOptions.ProviderType.SetDefaultValue(defaultProviderType);

var rootCommand = new RootCommand();
rootCommand.AddOption(AppOptions.ForceCli);
var configureCommand = new CliCommand("configure", "configure options for application");
configureCommand.SetHandler(Configure);
rootCommand.AddCommand(configureCommand);
rootCommand.AddCommand(Resolve<StreamCommand>());
rootCommand.AddCommand(Resolve<GrabCommand>());
rootCommand.AddCommand(Resolve<SearchCommand>());

if(args.Length > 0)
{
    return await rootCommand.InvokeAsync(args);
}
else
{
   return GuiRunner.Run();
}









T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

void Configure()
{
    var p = new Process();
    p.StartInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
    {
        UseShellExecute = true
    };
    p.Start();
}