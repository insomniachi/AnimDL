using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IStreamProvider
{
    public IAsyncEnumerable<HlsStreams> GetStreams(string url);
}
