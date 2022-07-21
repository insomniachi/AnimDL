using System.Text.Json.Serialization;

namespace MalApi
{
    public class Studio
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
