using System.Text.Json.Serialization;

namespace MalApi
{
    public class Picture
    {
        [JsonPropertyName("large")]
        public string Large { get; set; }

        [JsonPropertyName("medium")]
        public string Medium { get; set; }
    }
}
