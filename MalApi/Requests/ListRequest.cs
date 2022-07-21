using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalApi.Requests
{
    public abstract class ListAnimeRequest : HttpGetRequest<List<Anime>>
    {
        public int Count { get; set; } = 25;

        protected async override Task<List<Anime>> CreateResponse(string json)
        {
            List<Anime> result = new List<Anime>();

            AnimeListRoot root = JsonSerializer.Deserialize<AnimeListRoot>(json);
            result.AddRange(root.AnimeList.Select(x => x.Anime));

            if (result.Count < Count)
            {
                while (string.IsNullOrEmpty(root.Paging.Next) == false)
                {
                    var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                    string nextString = await nextResponse.Content.ReadAsStringAsync();

                    root = JsonSerializer.Deserialize<AnimeListRoot>(nextString);
                    result.AddRange(root.AnimeList.Select(x => x.Anime));

                    if (result.Count > Count)
                    {
                        break;
                    }
                } 
            }
            
            return result.Take(Count).ToList();
        }
    }

    public abstract class ListMangaRequest : HttpGetRequest<List<Manga>>
    {
        public int Count { get; set; } = 25;

        protected async override Task<List<Manga>> CreateResponse(string json)
        {
            List<Manga> result = new List<Manga>();

            MangaListRoot root = JsonSerializer.Deserialize<MangaListRoot>(json);
            result.AddRange(root.MangaList.Select(x => x.Manga));

            if (result.Count < Count)
            {
                while (string.IsNullOrEmpty(root.Paging.Next) == false)
                {
                    var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                    string nextString = await nextResponse.Content.ReadAsStringAsync();

                    root = JsonSerializer.Deserialize<MangaListRoot>(nextString);
                    result.AddRange(root.MangaList.Select(x => x.Manga));

                    if (result.Count > Count)
                    {
                        break;
                    }
                }
            }

            return result.Take(Count).ToList();
        }
    }
}
