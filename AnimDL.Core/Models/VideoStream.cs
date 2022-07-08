namespace AnimDL.Core.Models;

public class VideoStream
{
    public string Quality { get; set; } = string.Empty;
    public Dictionary<string,string> Headers { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}

public class VideoStreamsForEpisode
{
    public int Episode { get; set; }
    public Dictionary<string, VideoStream> Qualities { get; set; } = new();
}