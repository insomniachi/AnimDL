using AnimDL.Core.AiredEpisodesProvider;
using AnimDL.Core.StreamProviders;

namespace AnimDL.Core.Tests.StreamProviders
{
    public class YugenAnimeStreamProviderTests
    {
        private readonly HttpClient _httpClient = new();

        [Theory]
        [InlineData("https://yugen.to/anime/568/91-days/")]
        public async Task GetStreams_GivesResultFromCategoryPage(string url)
        {
            var provider = new YugenAnimeStreamProvider(_httpClient);
            var result = await provider.GetStreams(url, 1..1).ToListAsync();
        }

        [Fact]
        public async Task YugenAnimeEpisodes()
        {
            var provider = new YugenAnimeAiredEpisodesProvider();
            var result = await provider.GetRecentlyAiredEpisodes();
        }
    }
}
