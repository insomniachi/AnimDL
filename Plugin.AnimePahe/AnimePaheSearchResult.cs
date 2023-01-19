using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.AnimePahe;

internal class AnimePaheSearchResult : SearchResult, IHaveImage, IHaveSeason
{
    public string Image { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
