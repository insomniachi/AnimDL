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
                               new ProviderBinder(AppOptions.ProviderType),
                               AppOptions.Episode,
                               new ResolveBinder<ILogger<StreamCommand>>(),
                               new ResolveBinder<IMediaPlayer>());
            return command;
        }

        public static async Task Execute(string query, IProvider provider,
            int episode, ILogger<StreamCommand> logger, IMediaPlayer mediaPlayer)
        {
            if (mediaPlayer.IsAvailable == false)
            {
                logger.LogWarning("Media player not conifgured, use \"animdl configure\" to configure");
                return;
            }

            logger.LogInformation("Searching in {Type}", provider.ProviderType);

            var results = await provider.Catalog.Search(query).ToListAsync();
            var selectedResult = results.Count == 1 
                ? results[0] 
                : Prompt.Select("Select", results, textSelector: x => x.Title);
            
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
            await mediaPlayer.Play(url, $"{selectedResult.Title} - Episode {episode}");
        }
    }
}
