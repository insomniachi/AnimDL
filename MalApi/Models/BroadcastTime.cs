using System.Text.Json.Serialization;

namespace MalApi
{
    public class BroadcastTime
    {
        [JsonPropertyName("day_of_the_week")]
        public string DayOfWeek { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }
    }
}
