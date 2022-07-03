
using AnimDL.Commands;
using AnimDL.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.RegisterCommands()
                .RegisterProviders();
    })
    .Build();

var rootCommand = new RootCommand();
rootCommand.AddGlobalOption(AppOptions.ProviderType);
rootCommand.AddCommand(host.Services.GetRequiredService<GrabCommand>());

return await rootCommand.InvokeAsync(args);
