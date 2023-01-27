using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IStreamProvider
{
    public IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range);

    public Task<int> GetNumberOfStreams(string url);
}

public interface IMultiAudioStreamProvider
{
    public IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range, string streamType);
}
