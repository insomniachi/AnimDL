using AnimDL.Core.Models;

namespace AnimDL.Core.Api
{
    public interface IStreamExtractor
    {
        Task<HlsStreams> Extract(string url);
    }
}