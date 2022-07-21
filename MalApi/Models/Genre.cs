using System.Text.Json.Serialization;

namespace MalApi
{
    public class Genre
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
