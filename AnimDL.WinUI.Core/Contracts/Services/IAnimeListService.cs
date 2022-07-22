using System.Collections.Generic;
using System.Threading.Tasks;
using MalApi;

namespace AnimDL.WinUI.Core.Contracts.Services;
public interface IAnimeListService
{
    //Task<IEnumerable<Anime>> GetAnimeSeason(int year, Season season);
    Task<List<Anime>> GetAnimeSeason(int year, AnimeSeason season);
    Task<List<Anime>> GetUserAnime();
}
