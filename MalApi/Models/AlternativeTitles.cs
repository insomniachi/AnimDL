using System.Text.Json.Serialization;

namespace MalApi
{
    public class AlternativeTitles
    {
        [JsonPropertyName("synonyms")]
        public string[] Aliases { get; set; }

        [JsonPropertyName("en")]
        public string English { get; set; }

        [JsonPropertyName("ja")]
        public string Japanese { get; set; }
    }
}
