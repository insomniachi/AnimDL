namespace AnimDL.Core.Models;

public class HlsStreams
{
    public int episode { get; set; }
    public List<HlsStreamInfo> streams { get; set; } = new();
}

public class HlsStreamInfo
{
    public string quality { get; set; } = "Default";
    public RequestHeaders? headers { get; set; }
    public string stream_url { get; set; } = string.Empty;
}

public class RequestHeaders
{
    public string referer { get; set; } = string.Empty;
}
