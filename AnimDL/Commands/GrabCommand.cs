using AnimDL.Api;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;

namespace AnimDL.Commands
{
    public class GrabCommand : Command
    {
        private readonly IProviderFactory _providerFactory;
        private readonly ILogger _logger;

        public GrabCommand(IProviderFactory providerFactory, ILogger<GrabCommand> logger) : base("grab", "grabs stream links")
        {
            _providerFactory = providerFactory;
            _logger = logger;

            AddArgument(AppArguments.Title);
            this.SetHandler(Execute, AppArguments.Title, AppOptions.ProviderType);
        }

        public async Task Execute(string query, ProviderType providerType)
        {
            var provider = _providerFactory.GetProvider(providerType);
           
            _logger.LogInformation("Searching in {Type}", providerType);

            var results = await provider.Catalog.Search(query).ToListAsync();
            var count = 0;
            foreach (var item in results)
            {
                Console.WriteLine($"[{count++}] => {item.Title} ({item.Url})");
            }

            var selection = Prompt.GetUserInput("Select : ");
            var selectedResult = results[selection];

            await foreach (var item in provider.StreamProvider.GetStreams(selectedResult.Url))
            {
                Console.WriteLine(item.Format(providerType));
            }
        }
    }
}
