using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults;

internal class GogoAnimeSearchResult : SearchResult, IHaveImage, IHaveYear
{
    public string Image { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
