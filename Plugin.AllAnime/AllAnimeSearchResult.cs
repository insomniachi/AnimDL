using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.AllAnime;

public class AllAnimeSearchResult : SearchResult, IHaveImage, IHaveSeason, IHaveRating
{
    public string Season { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
}
