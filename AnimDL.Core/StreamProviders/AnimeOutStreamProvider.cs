using AnimDL.Core.Models;
using Microsoft.Extensions.Logging;

namespace AnimDL.Core.StreamProviders;

internal class AnimeOutStreamProvider : BaseStreamProvider
{
    private readonly ILogger<AnimeOutStreamProvider> logger;

    public AnimeOutStreamProvider(ILogger<AnimeOutStreamProvider> logger, HttpClient client) : base(client)
    {
        this.logger = logger;
    }

    public override IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url)
    {
        throw new NotImplementedException();
    }
}
