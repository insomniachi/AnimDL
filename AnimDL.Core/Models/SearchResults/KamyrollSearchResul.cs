using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults
{
    internal class KamyrollSearchResult : SearchResult, IHaveImage
    {
        public string Image { get; set; } = string.Empty;
    }
}
