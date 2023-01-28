using System.Text.Json;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;
using Xunit.Abstractions;

namespace AnimDL.Core.Tests;

public class ProviderTests
{
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _searializerOption = new() { WriteIndented = true };

    public ProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("allanime", "hyouka")]
    [InlineData("animepahe", "hyouka")]
    [InlineData("marin", "hyouka")]
    [InlineData("yugen", "hyouka")]
    [InlineData("gogo", "hyouka")]
    //[InlineData("kamy", "hyouka")]
    public async Task Catalog_Search(string providerType, string query)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);

        // act
        await AssertSearchResults(provider.Catalog.Search(query), providerType != "kamy");
    }

    [Theory]
    [InlineData("animepahe", "hyouka")]
    [InlineData("zoro", "hyouka")]
    [InlineData("enime", "hyouka")]
    [InlineData("crunchyroll", "hyouka")]
    public async Task Consumet_Catalog_Search(string providerType, string query)
    {
        Plugin.Consumet.Config.Provider = providerType;
        var provider = Helper.GetProvider("consumet");

        await AssertSearchResults(provider.Catalog.Search(query), false);
    }

    [Theory]
    [InlineData("allanime", "https://allanime.site/anime/dxxqKsaMhdrdQxczP", 22)]
    [InlineData("animepahe", "https://animepahe.com/anime/f2ed1711-0345-cadb-6b48-a626e44351c6", 22)]
    [InlineData("marin", "https://marin.moe/anime/4vvgviic", 22)]
    [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/", 22)]
    [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka", 22)]
    //[InlineData("kamy", "G6P585256", 22)]
    public async Task StreamProvider_GetNumberOfStreams(string providerType, string url, int expected)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);

        // act
        var actual = await provider.StreamProvider.GetNumberOfStreams(url);

        // assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("animepahe", "a2bc091e-7bc1-8438-f078-91eef648eaef", 22)]
    [InlineData("zoro", "hyouka-349", 22)]
    [InlineData("enime", "cl82p4w3o00247slu6qnobo40", 22)]
    [InlineData("crunchyroll", "G6P585256", 22)]
    public async Task Consumet_StreamProvider_GetNumberOfStreams(string providerType, string id, int expected)
    {
        // arrange
        Plugin.Consumet.Config.Provider = providerType;
        var provider = Helper.GetProvider("consumet");

        // act
        var actual = await provider.StreamProvider.GetNumberOfStreams(id);

        // assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("allanime", "https://allanime.site/anime/gHQe2eBBh57QdC9hZ")]
    //[InlineData("allanime", "https://allanime.site/anime/dxxqKsaMhdrdQxczP")]
    [InlineData("animepahe", "https://animepahe.com/anime/f2ed1711-0345-cadb-6b48-a626e44351c6")]
    [InlineData("marin", "https://marin.moe/anime/4vvgviic")]
    [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/")]
    [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka")]
    //[InlineData("kamy", "G6P585256")]
    public async Task StreamProvider_GetStreams(string providerType, string url)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);
        var count = 0;

        await foreach (var stream in provider.StreamProvider.GetStreams(url, 1..1))
        {
            _output.WriteLine(JsonSerializer.Serialize(stream, _searializerOption));
            Assert.NotEmpty(stream.Qualities);
            count++;
        }
    }

    [Theory]
    [InlineData("animepahe", "a2bc091e-7bc1-8438-f078-91eef648eaef")] // hyouka
    [InlineData("zoro", "hyouka-349")]
    [InlineData("crunchyroll", "G6P585256")] // hyouka
    [InlineData("crunchyroll", "G9VHN91DJ")] // angel next door
    public async Task Consumet_StreamProvider_GetStreams(string providerType, string id)
    {
        // arrange
        Plugin.Consumet.Config.Provider = providerType;
        var provider = Helper.GetProvider("consumet");
        var count = 0;

        await foreach (var stream in provider.StreamProvider.GetStreams(id, 1..1))
        {
            _output.WriteLine(JsonSerializer.Serialize(stream, _searializerOption));
            Assert.NotEmpty(stream.Qualities);
            count++;
        }
    }

    [Theory]
    [InlineData("allanime")]
    [InlineData("animepahe")]
    [InlineData("marin")]
    [InlineData("yugen")]
    [InlineData("gogo")]
    public async Task AiredEpisodesProvider_GetEpisodes(string providerType)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);

        await ActAndAssertRecentEpisodes(provider);
    }

    [Theory]
    [InlineData("zoro")]
    [InlineData("animepahe")]
    public async Task Consumet_AiredEpisodesProvider_GetEpisodes(string providerType)
    {
        // arrange
        Plugin.Consumet.Config.Provider = providerType;
        var provider = Helper.GetProvider("consumet");

        await ActAndAssertRecentEpisodes(provider);
    }


    private async Task ActAndAssertRecentEpisodes(IProvider provider)
    {
        var result = (await provider.AiredEpisodesProvider.GetRecentlyAiredEpisodes()).ToList();
        _output.WriteLine(JsonSerializer.Serialize(result, _searializerOption));
        Assert.NotEmpty(result);
    }

    private async Task AssertSearchResults(IAsyncEnumerable<SearchResult> results, bool assertUrl = true)
    {
        // act
        await foreach (var searchResult in results)
        {
            _output.WriteLine(JsonSerializer.Serialize(searchResult, searchResult.GetType(), _searializerOption));


            // assert
            Assert.False(string.IsNullOrEmpty(searchResult.Title));
            Assert.False(string.IsNullOrEmpty(searchResult.Url));

            if (searchResult is IHaveImage ihi)
            {
                Assert.False(string.IsNullOrEmpty(ihi.Image));
            }

            //if (searchResult is IHaveYear ihy)
            //{
            //    Assert.False(string.IsNullOrEmpty(ihy.Year));
            //}

            if (searchResult is IHaveSeason ihs)
            {
                Assert.False(string.IsNullOrEmpty(ihs.Season));
            }

            if (searchResult is IHaveType iht)
            {
                Assert.False(string.IsNullOrEmpty(iht.Type));
            }

            if (searchResult is IHaveGenre ihg)
            {
                Assert.False(string.IsNullOrEmpty(ihg.Genre));
            }

            if (searchResult is IHaveRating ihr)
            {
                Assert.False(string.IsNullOrEmpty(ihr.Rating));
            }

            if (searchResult is IHaveEpisodes ihe)
            {
                Assert.False(string.IsNullOrEmpty(ihe.Episodes));
            }

            if (assertUrl) // for kamy the url field will contain media id, so it's not a valid url
            {
                var result = await HttpHelper.Client.GetAsync(searchResult.Url);
                result.EnsureSuccessStatusCode();
            }
        }
    }

}
