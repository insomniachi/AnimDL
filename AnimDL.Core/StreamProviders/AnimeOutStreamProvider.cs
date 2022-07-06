using AnimDL.Core.Models;

namespace AnimDL.Core.StreamProviders;

public class AnimeOutStreamProvider : BaseStreamProvider
{
    public override IAsyncEnumerable<HlsStreams> GetStreams(string url)
    {
        throw new NotImplementedException();
    }
}
