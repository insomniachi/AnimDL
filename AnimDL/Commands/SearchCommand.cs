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
                           AppOptions.ProviderType, 
                           new ResolveBinder<IProviderFactory>(),
                           new ResolveBinder<ILogger<SearchCommand>>());

        return command;
    }


    public static async Task Execute(string query, ProviderType providerType, IProviderFactory providerFactory, ILogger<SearchCommand> logger)
    {
        var provider = providerFactory.GetProvider(providerType);

        logger.LogInformation("Searching in {Type}", providerType);

        var count = 1;
        await foreach (var item in provider.Catalog.Search(query))
        {
            logger.LogInformation("[{Index}] => {Title} ({Url})", count++, item.Title, item.Url);
        }
    }

    public static IAsyncEnumerable<SearchResult> UiExecute(string query, ProviderType providerType, IProviderFactory providerFactory)
    {
        var provider = providerFactory.GetProvider(providerType);
        return provider.Catalog.Search(query);
    }
}
