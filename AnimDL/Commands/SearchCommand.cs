using AnimDL.Api;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace AnimDL.Commands;

public class SearchCommand : Command
{
    private readonly IProviderFactory _providerFactory;
    private readonly ILogger _logger;

    public SearchCommand(IProviderFactory providerFactory,
                         ILogger<SearchCommand> logger) : base("search", "search anime on provider")
    {
        _providerFactory = providerFactory;
        _logger = logger;

        AddArgument(AppArguments.Title);
        AddOption(AppOptions.ProviderType);

        this.SetHandler(Execute, AppArguments.Title, AppOptions.ProviderType);
    }

    private async Task Execute(string query, ProviderType providerType)
    {
        var provider = _providerFactory.GetProvider(providerType);

        _logger.LogInformation("Searching in {Type}", providerType);

        var count = 1;
        await foreach (var item in provider.Catalog.Search(query))
        {
            _logger.LogInformation("[{Index}] => {Title} ({Url})", count++, item.Title, item.Url);
        }
    }
}
