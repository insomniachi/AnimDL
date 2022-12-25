using System.Net;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Splat;

namespace AnimDL.Core.StreamProviders;

public abstract class BaseStreamProvider : IStreamProvider, IEnableLogger
{
    protected readonly HtmlWeb _session = new();
    protected HttpStatusCode _statusCode = HttpStatusCode.OK;
    protected readonly HttpClient _client;

    public BaseStreamProvider(HttpClient client)
    {
        _session.PostResponse = (request, response) =>
        {
            if (response is not null)
            {
                _statusCode = response.StatusCode;
            }
        };
        _client = client;
    }

    protected Task<HtmlDocument> Load(string url)
    {
        return _session.LoadFromWebAsync(url);
    }

    public virtual IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range stream) => AsyncEnumerable.Empty<VideoStreamsForEpisode>();
    public virtual Task<int> GetNumberOfStreams(string url) => Task.FromResult(1);
}
