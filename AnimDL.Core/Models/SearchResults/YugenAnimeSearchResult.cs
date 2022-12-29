using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults;

internal class YugenAnimeSearchResult : SearchResult, IHaveImage, IHaveSeason, IHaveRating
{
    public string Rating { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
