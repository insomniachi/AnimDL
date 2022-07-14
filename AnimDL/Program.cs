using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core;
using AnimDL.Helpers;
using AnimDL.Media;
using AnimDL.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;
using CliCommand = System.CommandLine.Command;

namespace AnimDL;

public class Program
{

    private static readonly IHost _host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.AddAnimeDL();
        services.AddViews();
        services.AddMediaPlayers();
        DiscordRpc.Initialize();
    })
#if DEBUG
        .UseEnvironment("development")
#endif
    .Build();

    static async Task<int> Main(string[] args)
    {

        var config = _host.Services.GetRequiredService<IConfiguration>();
        var defaultProviderType = config.GetValue<ProviderType>("DefaultProvider");
        AppOptions.ProviderType.SetDefaultValue(defaultProviderType);

        var rootCommand = new RootCommand();
        var configureCommand = new CliCommand("configure", "configure options for application");
        configureCommand.SetHandler(Configure);
        rootCommand.AddCommand(configureCommand);
        rootCommand.AddCommand(GrabCommand.Create());
        rootCommand.AddCommand(SearchCommand.Create());
        rootCommand.AddCommand(StreamCommand.Create());

        if (args.Length > 0)
        {
            return await rootCommand.InvokeAsync(args);
        }
        else
        {
            return GuiRunner.Run();
        }

    }

    public static T Resolve<T>() where T : notnull => _host.Services.GetRequiredService<T>();

    public static void Navigate<TView, TViewModel>()
        where TView : BaseView<TViewModel>
    {
        var view = Resolve<TView>();
        view.Setup();
    }

    public static async Task ShowDialogAsync<TView,TViewModel>(Func<TViewModel,Task>? setup = null, Func<TViewModel, Task>? onActivated = null)
        where TView : BaseDialog<TViewModel>
    {
        var view = Resolve<TView>();
        if(setup is not null)
        {
            await setup(view.ViewModel);
        }
        if (onActivated is not null)
        {
            var task = onActivated(view.ViewModel);
        }
        view.ShowDialog();
    }

    public static void Configure()
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
            {
                UseShellExecute = true
            }
        };
        p.Start();
    }
}