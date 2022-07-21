using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class AnimeSourceConverter : JsonConverter<AnimeSource>
    {
        public override AnimeSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            switch (text)
            {
                case "other": return AnimeSource.Other;
                case "original": return AnimeSource.Original;
                case "manga": return AnimeSource.Manga;
                case "4_koma_manga": return AnimeSource.KomaManga;
                case "web_manga": return AnimeSource.WebManga;
                case "digital_manga": return AnimeSource.DigitalManga;
                case "novel": return AnimeSource.Novel;
                case "light_novel": return AnimeSource.LightNovel;
                case "visual_novel": return AnimeSource.VisualNovel;
                case "game": return AnimeSource.Game;
                case "card_game": return AnimeSource.CardGame;
                case "book": return AnimeSource.Book;
                case "picture_book": return AnimeSource.PictureBook;
                case "radio": return AnimeSource.Radio;
                case "music": return AnimeSource.Music;
                default: return AnimeSource.Other;
            }
        }

        public override void Write(Utf8JsonWriter writer, AnimeSource value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AnimeSource.Other:
                    writer.WriteStringValue("other");
                    break;
                case AnimeSource.Original:
                    writer.WriteStringValue("original");
                    break;
                case AnimeSource.Manga:
                    writer.WriteStringValue("manga");
                    break;
                case AnimeSource.KomaManga:
                    writer.WriteStringValue("4_koma_manga");
                    break;
                case AnimeSource.WebManga:
                    writer.WriteStringValue("web_manga");
                    break;
                case AnimeSource.DigitalManga:
                    writer.WriteStringValue("digital_manga");
                    break;
                case AnimeSource.Novel:
                    writer.WriteStringValue("novel");
                    break;
                case AnimeSource.LightNovel:
                    writer.WriteStringValue("light_novel");
                    break;
                case AnimeSource.VisualNovel:
                    writer.WriteStringValue("visual_novel");
                    break;
                case AnimeSource.Game:
                    writer.WriteStringValue("game");
                    break;
                case AnimeSource.CardGame:
                    writer.WriteStringValue("card_game");
                    break;
                case AnimeSource.Book:
                    writer.WriteStringValue("book");
                    break;
                case AnimeSource.PictureBook:
                    writer.WriteStringValue("picture_book");
                    break;
                case AnimeSource.Radio:
                    writer.WriteStringValue("radio");
                    break;
                case AnimeSource.Music:
                    writer.WriteStringValue("music");
                    break;
            }
        }
    }
}