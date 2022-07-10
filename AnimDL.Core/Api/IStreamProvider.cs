using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IStreamProvider
{
    public IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url);

    public Task<int> GetNumberOfStreams(string url);
}
