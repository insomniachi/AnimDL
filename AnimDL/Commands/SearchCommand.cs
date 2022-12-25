using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Reactive.Linq;
using AnimDL.Core.Helpers;
using AnimDL.Core.Api;

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


    public static void Execute(string query, IProvider provider, ILogger<SearchCommand> logger)
    {
        logger.LogInformation("Searching in {Type}", provider.ProviderType);

        var count = 1;
        var searchResult = provider.Catalog.Search(query).ToObservable();
        searchResult.Subscribe(OnNextSearchResult, OnError);
        searchResult.TryWait();


        void OnNextSearchResult(SearchResult result)
        {
            logger.LogInformation("[{Index}] => {Title} ({Url})", count++, result.Title, result.Url);
        }
        void OnError(Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

}
