using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.GogoAnime;

internal class GogoAnimeSearchResult : SearchResult, IHaveImage, IHaveYear
{
    public string Image { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
