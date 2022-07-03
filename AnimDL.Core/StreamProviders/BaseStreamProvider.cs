using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace AnimDL.Core.StreamProviders;

public abstract class BaseStreamProvider : IStreamProvider
{
    protected readonly HtmlWeb _session = new();
    protected HttpStatusCode _statusCode = HttpStatusCode.OK;
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public BaseStreamProvider()
    {
        _session.PostResponse = (request, response) =>
        {
            if (response is not null)
            {
                _statusCode = response.StatusCode;
            }
        };
    }

    protected async Task<HtmlDocument> Load(string url)
    {
        return await _cache.GetOrCreateAsync(url, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            return _session.LoadFromWebAsync(entry.Key.ToString());
        });
    }

    public abstract IAsyncEnumerable<HlsStreams> GetStreams(string url);
}
