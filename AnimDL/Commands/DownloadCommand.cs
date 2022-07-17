using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using Sharprompt;
using System.CommandLine;
using Xabe.FFmpeg;

namespace AnimDL.Commands;

internal class DownloadCommand
{
    public static Command Create()
    {
        var command = new Command("download", "downloads");
        command.AddArgument(AppArguments.Title);
        command.AddOption(AppOptions.ProviderType);
        command.AddOption(AppOptions.Range);

        command.SetHandler(Execute,
                           AppArguments.Title,
                           AppOptions.Range,
                           new ProviderBinder(AppOptions.ProviderType),
                           new ResolveBinder<ILogger<DownloadCommand>>(),
                           new ResolveBinder<IDownloader>());

        return command;
    }

    private static async Task Execute(string query, 
                                      Range range, 
                                      IProvider provider, 
                                      ILogger<DownloadCommand> logger,
                                      IDownloader downloader)
    {
        logger.LogInformation("Searching in {Type}", provider.ProviderType);

        var results = await provider.Catalog.Search(query).ToListAsync();

        SearchResult selectedResult;
        if (results.Count > 1)
        {
            selectedResult = Prompt.Select("Select", results, textSelector: x => x.Title, pageSize: 10);
        }
        else
        {
            selectedResult = results.First();
            logger.LogInformation("only 1 anime found, auto selecting.. {Title}", selectedResult.Title);
        }

        await foreach (var episodeStream in provider.StreamProvider.GetStreams(selectedResult.Url, range))
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

            await downloader.Download(stream.Url,
                                      selectedResult.Title,
                                      episodeStream.Episode.ToString().PadLeft(3,'0') + ".mp4",
                                      stream.Headers);

        }
    }
}
