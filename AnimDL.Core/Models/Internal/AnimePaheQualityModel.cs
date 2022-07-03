using System.Text.Json.Serialization;

namespace AnimDL.Core.Models.Internal;

public class AnimePaheQualityModel
{
    public List<Qualities> data { get; set; } = new();

    public Dictionary<string, Quality> GetQualities()
    {
        return data.Select(x => x.GetActive()).ToDictionary(x => x.Item1, x => x.Item2);
    }
}

public class Qualities
{
    [JsonPropertyName("360")]
    public Quality? QHD { get; set; }

    [JsonPropertyName("720")]
    public Quality? HD { get; set; }

    [JsonPropertyName("1080")]
    public Quality? FHD { get; set; }

    public (string, Quality) GetActive()
    {
        if (QHD is not null) return ("360", QHD);
        if (HD is not null) return ("720", HD);
        if (FHD is not null) return ("1080", FHD);
        return ("", new());
    }
}

public class Quality
{
    public int id { get; set; }
    public int filesize { get; set; }
    public string crc32 { get; set; } = string.Empty;
    public string revision { get; set; } = string.Empty;
    public string fansub { get; set; } = string.Empty;
    public string audio { get; set; } = string.Empty;
    public string disc { get; set; } = string.Empty;
    public int hq { get; set; }
    public string kwik { get; set; } = string.Empty;
    public string kwik_pahewin { get; set; } = string.Empty;
}
