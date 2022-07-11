using AnimDL.Api;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using Sharprompt;
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

            var results = await provider.Catalog.Search(query).ToListAsync();
            var selectedResult = Prompt.Select("Select", results, textSelector: x => x.Title);
            var episodeStream = await provider.StreamProvider.GetStreams(selectedResult.Url).ElementAtAsync(episode);

            if(!episodeStream.Qualities.Any())
            {
                logger.LogError("Didn't find any stream for {Query}", query);
            }

            var selectedQuality = episodeStream.Qualities.Keys.First();
            if(episodeStream.Qualities.Count > 1)
            {
                selectedQuality = Prompt.Select("Select", episodeStream.Qualities.Keys); 
            }

            var url = episodeStream.Qualities[selectedQuality].Url;
            mediaPlayer.Play(url);
        }
    }
}
