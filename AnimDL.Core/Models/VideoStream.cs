namespace AnimDL.Core.Models;

public class VideoStream
{
    public string Quality { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}

public class VideoStreamsForEpisode
{
    private int _episode;
    public int Episode
    {
        get => _episode;
        set
        {
            if(value > 0)
            {
                _episode = value;
                EpisodeString = value.ToString();
            }
        }
    }
    public string EpisodeString { get; set; } = string.Empty;
    public Dictionary<string, VideoStream> Qualities { get; set; } = new();
}