using AnimDL.Core.AiredEpisodesProvider;
using AnimDL.Core.Catalog;
using AnimDL.Core.Helpers;
using AnimDL.Core.StreamProviders;

namespace AnimDL.Core.Tests.StreamProviders;

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
        var result = (await provider.GetRecentlyAiredEpisodes()).ToList();
    }


    [Fact]
    public async Task Pahe()
    {
        var provider = new AnimePaheAiredEpisodesProvider(_httpClient);
        var result = (await provider.GetRecentlyAiredEpisodes()).ToList();
    }

    [Fact]
    public async Task AllAnime()
    {
        //var url = "https://allanime.site/anime/5FYQwxEPq4YdppRKK"; // bocchi
        var url = "https://allanime.site/anime/dxxqKsaMhdrdQxczP"; // hyouka
        var provider = new AllAnimeStreamProvider(_httpClient);
        var result = await provider.GetStreams(url, 1..1).ToListAsync();
    }

    [Fact]
    public async Task AllAnimeCatalog()
    {
        var c = new AllAnimeCatalog(_httpClient);
        var result = await c.Search("hyouk").ToListAsync(); 
    }

    [Fact]
    public async Task AllAnimeEpisodes()
    {
        var c = new AllAnimeAiredEpisodesProvider();
        var result = (await c.GetRecentlyAiredEpisodes()).ToList();
    }
}
