using System.Text.RegularExpressions;

namespace AnimDL.Core.Models;

public abstract partial class AiredEpisode
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    public virtual int GetEpisode()
    {
        var epMatch = EpisodeRegex().Match(Url);
        return epMatch.Success ? int.Parse(epMatch.Groups[1].Value) : 0;
    }

    [GeneratedRegex("(\\d+)", RegexOptions.RightToLeft)]
    private static partial Regex EpisodeRegex();
}

public interface IHaveCreatedTime
{
    DateTime CreatedTime { get; set; }
}
