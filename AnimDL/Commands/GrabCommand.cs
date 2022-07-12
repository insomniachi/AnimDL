﻿using AnimDL.Api;
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
                               new ProviderBinder(AppOptions.ProviderType),
                               new ResolveBinder<ILogger<GrabCommand>>());

            return command;
        }

        public static async Task Execute(string query, IProvider provider, ILogger<GrabCommand> logger)
        {
            logger.LogInformation("Searching in {Type}", provider.ProviderType);

            var results = await provider.Catalog.Search(query).ToListAsync();

            var selectedResult = results.Count == 1 
                ? results[0] 
                : Prompt.Select("Select", results, textSelector: x => x.Title);

            await foreach(var stream in provider.StreamProvider.GetStreams(selectedResult.Url))
            {
                Console.WriteLine(StreamOutputFormater.Format(stream, provider.ProviderType));
            }
        }
    }
}
