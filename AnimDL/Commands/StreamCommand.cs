using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Helpers;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace AnimDL.Commands
{
    public class StreamCommand : Command
    {
        private readonly IProviderFactory _providerFactory;
        private readonly ILogger _logger;
        private readonly IMediaPlayer _mediaPlayer;

        public StreamCommand(IProviderFactory providerFactory,
            ILogger<StreamCommand> logger,
            IMediaPlayer mediaPlayer) : base("stream", "stream anime")
        {
            _providerFactory = providerFactory;
            _logger = logger;
            _mediaPlayer = mediaPlayer;
            AddArgument(AppArguments.Title);
            AddOption(AppOptions.ProviderType);
            AddOption(AppOptions.Episode);

            this.SetHandler(Execute, AppArguments.Title, AppOptions.ProviderType, AppOptions.Episode);
        }

        public async Task Execute(string query, ProviderType providerType, int episode)
        {
            if (_mediaPlayer.IsAvailable == false)
            {
                _logger.LogWarning("Media player not conifgured, use \"animdl configure\" to configure");
                return;
            }

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

            var episodeStream = await provider.StreamProvider.GetStreams(selectedResult.Url).ElementAtAsync(episode);

            if(!episodeStream.Qualities.Any())
            {
                _logger.LogError("Didn't find any stream for {Query}", query);
            }

            selection = 0;
            if(episodeStream.Qualities.Count > 1)
            {
                var count = 0;
                foreach (var quality in episodeStream.Qualities.Keys)
                {
                    _logger.LogInformation("[{Index}] => {Quality}", count++, quality);
                }

                selection = Prompt.GetUserInput("Select : "); 
            }

            var url = episodeStream.Qualities.ElementAt(selection).Value.Url;
            _mediaPlayer.Play(url);
        }
    }
}
