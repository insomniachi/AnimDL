using AnimDL.Core.Models;

namespace AnimDL.Core.StreamProviders;

internal class AnimeOutStreamProvider : BaseStreamProvider
{
    public override IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url)
    {
        throw new NotImplementedException();
    }
}
