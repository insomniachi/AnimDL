using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IStreamProvider
{
    public IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url);
}
