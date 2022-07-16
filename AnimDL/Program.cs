using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Core;
using AnimDL.Helpers;
using AnimDL.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Reflection;

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
    })
    .ConfigureAppConfiguration(config => 
    {
        config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        config.AddJsonFile("appsettings.json");
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

        if(config.GetValue<bool>("UseRichPresense"))
        {
            DiscordRpc.Initialize();
        }

        var rootCommand = new RootCommand();
        rootCommand.AddCommand(ConfigureCommand.Create());
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
}