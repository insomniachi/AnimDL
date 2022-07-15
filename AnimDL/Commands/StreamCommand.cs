using AnimDL.Api;
using AnimDL.Core.Models;
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
            command.AddOption(AppOptions.Range);
            command.AddOption(AppOptions.MediaPlayer);
            command.SetHandler(Execute, 
                               AppArguments.Title,
                               new ProviderBinder(AppOptions.ProviderType),
                               AppOptions.Range,
                               new ResolveBinder<ILogger<StreamCommand>>(),
                               new MediaPlayerBinder(AppOptions.MediaPlayer));
            return command;
        }

        public static async Task Execute(string query, IProvider provider,
            Range eps, ILogger<StreamCommand> logger, IMediaPlayer mediaPlayer)
        {
            if (mediaPlayer.IsAvailable == false)
            {
                logger.LogWarning("Media player not conifgured, use \"animdl configure\" to configure");
                return;
            }

            logger.LogInformation("Searching in {Type}", provider.ProviderType);

            var results = await provider.Catalog.Search(query).ToListAsync();
            
            SearchResult selectedResult;
            if(results.Count > 1)
            {
                selectedResult = Prompt.Select("Select", results, textSelector: x => x.Title, pageSize: 10);
            }
            else
            {
                selectedResult = results.First();
                logger.LogInformation("only 1 anime found, auto selecting.. {Title}", selectedResult.Title);
            }

            await foreach(var episodeStream in provider.StreamProvider.GetStreams(selectedResult.Url).Slice(eps))
            {
                if (!episodeStream.Qualities.Any())
                {
                    logger.LogError("Didn't find any stream for {Query}", query);
                }

                string selectedQuality;
                if (episodeStream.Qualities.Count > 1)
                {
                    selectedQuality = Prompt.Select("Select", episodeStream.Qualities.Keys);
                }
                else
                {
                    selectedQuality = episodeStream.Qualities.Keys.First();
                    logger.LogInformation("only 1 quality found, selecting quality {Quality}", selectedQuality);
                }



                var stream = episodeStream.Qualities[selectedQuality];
                var title = $"{selectedResult.Title} - Episode {episodeStream.Episode}";

                DiscordRpc.SetPresense(new DiscordRPC.RichPresence()
                          .WithState("Watching")
                          .WithDetails(title));

                await mediaPlayer.Play(stream, title);

                Console.Clear();
            }
        }
    }
}
