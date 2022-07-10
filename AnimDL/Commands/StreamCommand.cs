using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace AnimDL.Commands
{
    public class StreamCommand
    {
        public static Command Create()
        {
            var command = new Command("stream", "stream anime");
            command.AddArgument(AppArguments.Title);
            command.AddOption(AppOptions.ProviderType);
            command.AddOption(AppOptions.Episode);
            command.SetHandler(Execute, 
                               AppArguments.Title,
                               AppOptions.ProviderType,
                               AppOptions.Episode,
                               new ResolveBinder<IProviderFactory>(),
                               new ResolveBinder<ILogger<StreamCommand>>(),
                               new ResolveBinder<IMediaPlayer>());
            return command;
        }

        public static async Task Execute(string query, ProviderType providerType, int episode,
                                         IProviderFactory providerFactory, ILogger<StreamCommand> logger, IMediaPlayer mediaPlayer)
        {
            if (mediaPlayer.IsAvailable == false)
            {
                logger.LogWarning("Media player not conifgured, use \"animdl configure\" to configure");
                return;
            }

            var provider = providerFactory.GetProvider(providerType);

            logger.LogInformation("Searching in {Type}", providerType);

            var results = new List<SearchResult>();
            await foreach (var item in provider.Catalog.Search(query))
            {
                logger.LogInformation("[{Index}] => {Title} ({Url})", results.Count, item.Title, item.Url);
                results.Add(item);
            }

            var selection = Prompt.GetUserInput("Select : ");
            var selectedResult = results[selection];

            var episodeStream = await provider.StreamProvider.GetStreams(selectedResult.Url).ElementAtAsync(episode);

            if(!episodeStream.Qualities.Any())
            {
                logger.LogError("Didn't find any stream for {Query}", query);
            }

            selection = 0;
            if(episodeStream.Qualities.Count > 1)
            {
                var count = 0;
                foreach (var quality in episodeStream.Qualities.Keys)
                {
                    logger.LogInformation("[{Index}] => {Quality}", count++, quality);
                }

                selection = Prompt.GetUserInput("Select : "); 
            }

            var url = episodeStream.Qualities.ElementAt(selection).Value.Url;
            mediaPlayer.Play(url);
        }
    }
}
