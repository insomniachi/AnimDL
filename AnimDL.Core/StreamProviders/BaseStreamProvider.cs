using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace AnimDL.Core.StreamProviders;

internal abstract class BaseStreamProvider : IStreamProvider
{
    protected readonly HtmlWeb _session = new();
    protected HttpStatusCode _statusCode = HttpStatusCode.OK;
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
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

    protected async Task<HtmlDocument> Load(string url)
    {
        return await _cache.GetOrCreateAsync(url, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            return _session.LoadFromWebAsync(entry.Key.ToString());
        });
    }

    public abstract IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url);
    public virtual Task<int> GetNumberOfStreams(string url) => Task.FromResult(1);
}
