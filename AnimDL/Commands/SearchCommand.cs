using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace AnimDL.Commands;

public class SearchCommand
{
    public static Command Create()
    {
        var command = new Command("search", "search anime on provider");
        command.AddArgument(AppArguments.Title);
        command.AddOption(AppOptions.ProviderType);

        command.SetHandler(Execute, 
                           AppArguments.Title, 
                           new ProviderBinder(AppOptions.ProviderType), 
                           new ResolveBinder<ILogger<SearchCommand>>());

        return command;
    }


    public static async Task Execute(string query, IProvider provider, ILogger<SearchCommand> logger)
    {
        logger.LogInformation("Searching in {Type}", provider.ProviderType);

        var count = 1;
        await foreach (var item in provider.Catalog.Search(query))
        {
            logger.LogInformation("[{Index}] => {Title} ({Url})", count++, item.Title, item.Url);
        }
    }

    public static IAsyncEnumerable<SearchResult> UiExecute(string query, IProvider provider)
    {
        return provider.Catalog.Search(query);
    }
}
