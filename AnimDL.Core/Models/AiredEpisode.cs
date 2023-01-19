using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AnimDL.Core.Models;

[DebuggerDisplay("{Title} - {Episode}")]
public abstract partial class AiredEpisode
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Episode { get; set; }
    public string EpisodeString { get; set; } = string.Empty;

    [GeneratedRegex("(\\d+)", RegexOptions.RightToLeft)]
    public static partial Regex EpisodeRegex();
}

public interface IHaveCreatedTime
{
    DateTime CreatedAt { get; set; }
}

public interface IHaveHumanizedCreatedTime
{
    string CreatedAtHumanized { get; set; }
}
