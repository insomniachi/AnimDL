using System.Text.Json.Serialization;

namespace MalApi
{
    public class Ranking
    {
        [JsonPropertyName("rank")]
        public int CurrentRank { get; set; }

        [JsonPropertyName("previous_rank")]
        public int? PreviousRank { get; set; }
    }
}
