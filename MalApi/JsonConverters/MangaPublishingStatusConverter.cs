using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class MangaPublishingStatusConverter : JsonConverter<MangaPublishingStatus>
    {
        public override MangaPublishingStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();

            switch(value)
            {
                case "finished": return MangaPublishingStatus.Finished;
                case "currently_publishing": return MangaPublishingStatus.CurrentlyPublising;
                case "not_yet_published": return MangaPublishingStatus.NotYetPublished;
                default: return MangaPublishingStatus.NotYetPublished;
            }
        }

        public override void Write(Utf8JsonWriter writer, MangaPublishingStatus value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MangaPublishingStatus.Finished:
                    writer.WriteStringValue("finished");
                    break;
                case MangaPublishingStatus.CurrentlyPublising:
                    writer.WriteStringValue("currently_publishing");
                    break;
                case MangaPublishingStatus.NotYetPublished:
                    writer.WriteStringValue("not_yet_published");
                    break;
            }
        }
    }
}