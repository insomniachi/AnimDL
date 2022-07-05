using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace AnimDL.Commands
{
    public class GrabCommand : Command
    {
        private readonly IProviderFactory _providerFactory;
        private readonly ILogger _logger;

        public GrabCommand(IProviderFactory providerFactory,
            ILogger<StreamCommand> logger) : base("grab", "grab stream links")
        {
            _providerFactory = providerFactory;
            _logger = logger;
            AddArgument(AppArguments.Title);
            AddOption(AppOptions.ProviderType);

            this.SetHandler(Execute, AppArguments.Title, AppOptions.ProviderType);
        }

        public async Task Execute(string query, ProviderType providerType)
        {
            var provider = _providerFactory.GetProvider(providerType);

            _logger.LogInformation("Searching in {Type}", providerType);

            var results = new List<SearchResult>();
            await foreach (var item in provider.Catalog.Search(query))
            {
                _logger.LogInformation("[{Index}] => {Title} ({Url})", results.Count, item.Title, item.Url);
                results.Add(item);
            }

            var selection = Prompt.GetUserInput("Select : ");
            var selectedResult = results[selection];

            await foreach(var stream in provider.StreamProvider.GetStreams(selectedResult.Url))
            {
                Console.WriteLine(StreamOutputFormater.Format(stream, providerType));
            }
        }
    }
}
