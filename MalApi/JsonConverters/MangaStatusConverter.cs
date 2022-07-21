using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class MangaStatusConverter : JsonConverter<MangaStatus>
    {
        public override MangaStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string status = reader.GetString();

            switch (status)
            {
                case "completed": return MangaStatus.Completed;
                case "dropped": return MangaStatus.Dropped;
                case "on_hold": return MangaStatus.OnHold;
                case "plan_to_read": return MangaStatus.PlanToRead;
                case "reading": return MangaStatus.Reading;
                default: return MangaStatus.None;
            }
        }

        public override void Write(Utf8JsonWriter writer, MangaStatus value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MangaStatus.Completed:
                    writer.WriteStringValue("completed");
                    break;
                case MangaStatus.Dropped:
                    writer.WriteStringValue("dropped");
                    break;
                case MangaStatus.OnHold:
                    writer.WriteStringValue("on_hold");
                    break;
                case MangaStatus.PlanToRead:
                    writer.WriteStringValue("plan_to_read");
                    break;
                case MangaStatus.Reading:
                    writer.WriteStringValue("reading");
                    break;
            }
        }
    }
}
