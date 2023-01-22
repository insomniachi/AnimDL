using System.Diagnostics;
using System.Text.Json;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models.Interfaces;
using Xunit.Abstractions;

namespace AnimDL.Core.Tests
{
    public class ProviderTests
    {
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _searializerOption = new() { WriteIndented = true };
        private readonly string kamyToken = @"O+xmBPFx1UxoAiQYjDc9YYq01SdCZo1ABBoHDrNuIScEIKmYfIZoj57l1xeoLWGW3R2ZlxPlyqUf5R3hWzx+xSQnmPyk3GoUIFF19P0oCqp2B9ivNhtYiqir06rBK71mRzIjVUCmN3C7MvQUhH82QWUfZiCpFzCvxJ8cIX4Jr8w=";

        public ProviderTests(ITestOutputHelper output)
        {
            _output = output;
            Plugin.KamyRoll.Config.AccessToken = kamyToken;
            Plugin.KamyRoll.KamyRollClient.SetAccessToken();
        }

        [Theory]
        [InlineData("allanime", "hyouka")]
        [InlineData("animepahe", "hyouka")]
        [InlineData("marin", "hyouka")]
        [InlineData("yugen", "hyouka")]
        [InlineData("gogo", "hyouka")]
        [InlineData("kamy", "hyouka")]
        public async Task Catalog_Search(string providerType, string query)
        {
            // arrange
            var provider = Helper.GetProvider(providerType);

            // act
            await foreach (var searchResult in provider.Catalog.Search(query))
            {
                _output.WriteLine(JsonSerializer.Serialize(searchResult, searchResult.GetType(), _searializerOption));


                // assert
                Assert.False(string.IsNullOrEmpty(searchResult.Title));
                Assert.False(string.IsNullOrEmpty(searchResult.Url));

                if(searchResult is IHaveImage ihi)
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

                if(searchResult is IHaveType iht)
                {
                    Assert.False(string.IsNullOrEmpty(iht.Type));
                }

                if(searchResult is IHaveGenre ihg)
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

                if (providerType != "kamy") // for kamy the url field will contain media id, so it's not a valid url
                {
                    var result = await HttpHelper.Client.GetAsync(searchResult.Url);
                    result.EnsureSuccessStatusCode(); 
                }
            }
        }

        [Theory]
        [InlineData("allanime", "https://allanime.site/anime/dxxqKsaMhdrdQxczP", 22)]
        [InlineData("animepahe", "https://animepahe.com/anime/f2ed1711-0345-cadb-6b48-a626e44351c6", 22)]
        [InlineData("marin", "https://marin.moe/anime/4vvgviic", 22)]
        [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/", 22)]
        [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka", 22)]
        [InlineData("kamy", "G6P585256", 22)]
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
        [InlineData("allanime", "https://allanime.site/anime/gHQe2eBBh57QdC9hZ")]
        //[InlineData("allanime", "https://allanime.site/anime/dxxqKsaMhdrdQxczP")]
        [InlineData("animepahe", "https://animepahe.com/anime/f2ed1711-0345-cadb-6b48-a626e44351c6")]
        [InlineData("marin", "https://marin.moe/anime/4vvgviic")]
        [InlineData("yugen", "https://yugen.to/anime/2016/hyouka/")]
        [InlineData("gogo", "https://www1.gogoanime.bid/category/hyouka")]
        [InlineData("kamy", "G6P585256")]
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
    }
}
