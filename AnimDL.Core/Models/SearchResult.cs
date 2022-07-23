namespace AnimDL.Core.Models;

public class SearchResult
{
    public int Count { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Title}";
    }
}
