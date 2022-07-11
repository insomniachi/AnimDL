using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using Sharprompt;
using System.CommandLine;

namespace AnimDL.Commands
{
    public class GrabCommand
    {
        public static Command Create()
        {
            var command = new Command("grab", "grabs stream links");
            command.AddArgument(AppArguments.Title);
            command.AddOption(AppOptions.ProviderType);

            command.SetHandler(Execute, 
                               AppArguments.Title,
                               AppOptions.ProviderType,
                               new ResolveBinder<IProviderFactory>(),
                               new ResolveBinder<ILogger<GrabCommand>>());

            return command;
        }

        public static async Task Execute(string query, ProviderType providerType, IProviderFactory providerFactory, ILogger<GrabCommand> logger)
        {
            var provider = providerFactory.GetProvider(providerType);

            logger.LogInformation("Searching in {Type}", providerType);

            var results = new List<SearchResult>();
            await foreach (var item in provider.Catalog.Search(query))
            {
                logger.LogInformation("[{Index}] => {Title} ({Url})", results.Count, item.Title, item.Url);
                results.Add(item);
            }

            var selection = Prompt.Input<int>("Select");
            var selectedResult = results[selection];

            await foreach(var stream in provider.StreamProvider.GetStreams(selectedResult.Url))
            {
                Console.WriteLine(StreamOutputFormater.Format(stream, providerType));
            }
        }
    }
}
