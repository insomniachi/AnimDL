﻿using System.Text.Json;
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
    [InlineData("animepahe", "danmachi")]
    [InlineData("marin", "hyouka")]
    [InlineData("yugen", "hyouka")]
    [InlineData("gogo", "hyouka")]
    public async Task Catalog_Search(string providerType, string query)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);

        // act
        await AssertSearchResults(provider.Catalog.Search(query), false);
    }

    [Theory]
    [InlineData("zoro", "hyouka")]
    [InlineData("enime", "hyouka")]
    [InlineData("crunchyroll", "hyouka")]
    [InlineData("gogoanime", "hyouka")]
    public async Task Consumet_Catalog_Search(string providerType, string query)
    {
        Plugin.Consumet.Config.Provider = providerType;
        var provider = Helper.GetProvider("consumet");

        await AssertSearchResults(provider.Catalog.Search(query), false);
    }

    [Theory]
    [InlineData("allanime", "https://allanime.to/anime/dxxqKsaMhdrdQxczP", 22)]
    [InlineData("animepahe", "https://animepahe.com/anime/be63efaf-508a-74cb-c1b0-dbe371bc1d47", 22)]
    [InlineData("marin", "https://marin.moe/anime/4vvgviic", 22)]
    [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/", 22)]
    [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka", 22)]
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
    [InlineData("allanime", "https://allanime.to/anime/worqyo39hvnMwZXbe", "sub", 4)]
    public async Task MultiAudioStreamProvider_StreamProvider_GetNumberOfStreams(string providerType, string url, string type, int expected)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);
        var streamProvider = provider.StreamProvider as IMultiAudioStreamProvider;
        
        // act
        var actual = await streamProvider.GetNumberOfStreams(url, type);

        // assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("animepahe", "a2bc091e-7bc1-8438-f078-91eef648eaef", 22)]
    [InlineData("zoro", "hyouka-349", 22)]
    [InlineData("enime", "cl82p4w3o00247slu6qnobo40", 22)]
    [InlineData("crunchyroll", "G6P585256", 22)]
    [InlineData("gogoanime", "hyouka", 22)]
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
    [InlineData("allanime", "https://allanime.to/anime/dxxqKsaMhdrdQxczP")]
    [InlineData("allanime", "https://allanime.to/anime/3QCFtixZM7P628ANS")]
    [InlineData("allanime", "https://allanime.to/GPGmNh83WhsBPLCmE")]
    [InlineData("animepahe", "https://animepahe.com/anime/be63efaf-508a-74cb-c1b0-dbe371bc1d47")]
    [InlineData("animepahe", "https://animepahe.com/anime/6dd1970c-169b-d73f-9926-8483baf63f9a")]
    [InlineData("animepahe", "https://animepahe.com/anime/94ba7e1f-e373-7aad-41e0-4c1181ec1df3")]
    [InlineData("marin", "https://marin.moe/anime/4vvgviic")]
    [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/")]
    [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka")]
    public async Task StreamProvider_GetStreams(string providerType, string url)
    {
        // arrange
        var provider = Helper.GetProvider(providerType);
        var count = 0;

        await foreach (var stream in provider.StreamProvider.GetStreams(url, Range.All))
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
    [InlineData("crunchyroll", "G6NQ5DWZ6")] // mha
    [InlineData("crunchyroll", "GR751KNZY")] // attack on tita
    [InlineData("gogoanime", "hyouka")]
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
    [InlineData("gogoanime")]
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
        foreach (var item in result)
        {
            _output.WriteLine(JsonSerializer.Serialize(item, item.GetType(), _searializerOption));
        }
        Assert.NotEmpty(result);
    }

    private async Task AssertSearchResults(IAsyncEnumerable<SearchResult> results, bool assertUrl = false)
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
