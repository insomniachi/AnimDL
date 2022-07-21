using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class AnimeMediaTypeConverter : JsonConverter<AnimeMediaType>
    {
        public override AnimeMediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            switch (text)
            {
                case "unknown": return AnimeMediaType.Unknown;
                case "tv": return AnimeMediaType.TV;
                case "ova": return AnimeMediaType.OVA;
                case "movie": return AnimeMediaType.Movie;
                case "special": return AnimeMediaType.Special;
                case "ona": return AnimeMediaType.ONA;
                case "music": return AnimeMediaType.Music;
                default: return AnimeMediaType.Unknown;
            }
        }

        public override void Write(Utf8JsonWriter writer, AnimeMediaType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLower());
        }
    }
}
