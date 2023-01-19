using Plugin.GogoAnime;

namespace AnimDL.Core.Tests.StreamProviders
{
    public class GogoAnimeStreamProviderTests
    {
        private readonly HttpClient _httpClient = new();

        [Theory]
        [InlineData("https://gogoanime.ar/category/serial-experiments-lain")]
        public async Task GetStreams_GivesResultFromCategoryPage(string url)
        {
            var provider = new GogoAnimeStreamProvider(new Extractors.GogoPlayExtractor(), _httpClient);

            var result = await provider.GetStreams(url, 1..1).ToListAsync();

            Assert.Single(result);
            Assert.Equal(1, result[0].Episode);
        }
    }
}
