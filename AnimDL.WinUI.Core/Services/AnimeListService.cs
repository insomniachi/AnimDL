using AnimDL.WinUI.Core.Contracts.Services;
using MalApi;

namespace AnimDL.WinUI.Core.Services;

public class AnimeListService : IAnimeListService
{
    private readonly MalClient _client;

    public AnimeListService(string token)
    {
        _client = new MalClient(token);
    }

    public async Task<List<Anime>> GetAnimeSeason(int year, AnimeSeason season)
    {
        var result = await _client.GetSeasonalAnimeAsync(season, year);
        return result;
    }

    public async Task<List<Anime>> GetUserAnime()
    {
        return await _client.GetUserAnimeAsync();
    }

}
