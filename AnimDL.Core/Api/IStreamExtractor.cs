using AnimDL.Core.Models;

namespace AnimDL.Core.Api
{
    public interface IStreamExtractor
    {
        Task<VideoStreamsForEpisode?> Extract(string url);
    }
}