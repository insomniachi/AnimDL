using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class MangaMediaTypeConverter : JsonConverter<MangaMediaType>
    {
        public override MangaMediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            switch (text)
            {
                case "unknown": return MangaMediaType.Unknown;
                case "manga": return MangaMediaType.Manga;
                case "novel": return MangaMediaType.Novel;
                case "one_shot": return MangaMediaType.OneShot;
                case "doujinshi": return MangaMediaType.Doujinshi;
                case "manhwa": return MangaMediaType.Manhwa;
                case "manhua": return MangaMediaType.Manhua;
                case "oel": return MangaMediaType.Oel;
                default: return MangaMediaType.Unknown;
            }
        }

        public override void Write(Utf8JsonWriter writer, MangaMediaType value, JsonSerializerOptions options)
        {
            if(value == MangaMediaType.OneShot)
            {
                writer.WriteStringValue("one_shot");
            }
            else
            {
                writer.WriteStringValue(value.ToString().ToLower());
            }
        }
    }
}
