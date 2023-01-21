using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.Yugen;

public class YugenAnimeSearchResult : SearchResult, IHaveImage, IHaveSeason, IHaveRating
{
    public string Rating { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
