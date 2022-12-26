using System.Text.RegularExpressions;

namespace AnimDL.Core.Models;

public abstract partial class AiredEpisode
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Episode { get; set; }

    [GeneratedRegex("(\\d+)", RegexOptions.RightToLeft)]
    internal static partial Regex EpisodeRegex();
}

public interface IHaveCreatedTime
{
    DateTime CreatedAt { get; set; }
}
