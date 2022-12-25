using System.Text.Json.Serialization;

namespace AnimDL.Core.Models.Internal;

internal class AnimePaheEpisodePage
{
    public int total { get; set; }
    public int per_page { get; set; }
    public int current_page { get; set; }
    public int last_page { get; set; }
    public string next_page_url { get; set; } = string.Empty;
    public string prev_page_url { get; set; } = string.Empty;
    public int from { get; set; }
    public int to { get; set; }
    public List<AnimePaheEpisodeInfo> data { get; set; } = new();
}

internal class AnimePaheEpisodeInfo
{
    public int id { get; set; }
    public int anime_id { get; set; }
    public int episode { get; set; }
    public int episode2 { get; set; }
    public string edition { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string snapshot { get; set; } = string.Empty;
    public string disc { get; set; } = string.Empty;
    public string duration { get; set; } = string.Empty;
    public string session { get; set; } = string.Empty;
    public int filler { get; set; }
    public string created_at { get; set; } = string.Empty;
}

internal class AnimePaheQualityModel
{
    public List<Qualities> data { get; set; } = new();

    public Dictionary<string, Quality> GetQualities(string audio = "jpn")
    {
        return data.Where(x => x.GetActive().Item2.audio == audio)
            .Select(x => x.GetActive())
            .ToDictionary(x => x.Item1, x => x.Item2);
    }
}

internal class Qualities
{
    [JsonPropertyName("360")]
    public Quality? QHD { get; set; }

    [JsonPropertyName("720")]
    public Quality? HD { get; set; }

    [JsonPropertyName("1080")]
    public Quality? FHD { get; set; }

    public (string, Quality) GetActive()
    {
        if (QHD is not null)
            return ("360", QHD);
        if (HD is not null)
            return ("720", HD);
        if (FHD is not null)
            return ("1080", FHD);
        return ("", new());
    }
}

internal class Quality
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
