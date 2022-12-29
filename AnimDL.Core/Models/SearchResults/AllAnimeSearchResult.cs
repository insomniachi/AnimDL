using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults;

internal class AllAnimeSearchResult : SearchResult, IHaveImage, IHaveSeason, IHaveRating
{
    public string Season { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
}
