using System.Globalization;
using System.Text.Json.Serialization;

namespace MalApi
{
    public class Season
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("season")]
        public string SeasonName { get; set; }

        public override string ToString()
        {
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            return $"{info.ToTitleCase(SeasonName)} {Year}";
        }

        public static Season Get(string season, int year)
        {
            return new Season { Year = year, SeasonName = season };
        }

        public static Season Get(AnimeSeason season, int year)
        {
            return Get(season.ToString().ToLower(), year);
        }
    }
}
