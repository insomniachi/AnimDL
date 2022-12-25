using System.Net;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Splat;

namespace AnimDL.Core.StreamProviders;

public abstract class BaseStreamProvider : IStreamProvider, IEnableLogger
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

#nullable disable
    protected async Task<HtmlDocument> Load(string url)
    {
        return await _cache.GetOrCreateAsync(url, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            return _session.LoadFromWebAsync(entry.Key.ToString());
        });
    }
#nullable enable

    public virtual IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range stream) => AsyncEnumerable.Empty<VideoStreamsForEpisode>();
    public virtual Task<int> GetNumberOfStreams(string url) => Task.FromResult(1);
}
