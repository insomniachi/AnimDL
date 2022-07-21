using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalApi.Requests
{
    public class GetRankedMangaRequest : HttpGetRequest<List<RankedManga>>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/manga/ranking";

        public int Count { get; set; }

        public GetRankedMangaRequest(MangaRankingType type = MangaRankingType.All, int count = 25)
        {
            Parameters.Add("ranking_type",type.ToString().ToLower());
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_volumes,num_chapters,authors{first_name,last_name},pictures,background,related_anime,related_manga,recommendations,serialization{name}");
            Count = count;
        }

        protected async override Task<List<RankedManga>> CreateResponse(string json)
        {
            List<RankedManga> result = new List<RankedManga>();

            MangaListRoot root = JsonSerializer.Deserialize<MangaListRoot>(json);
            result.AddRange(root.MangaList.Select(x => new RankedManga { Ranking = x.Ranking, Manga = x.Manga }));

            if (result.Count < Count)
            {
                while (string.IsNullOrEmpty(root.Paging.Next) == false)
                {
                    var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                    string nextString = await nextResponse.Content.ReadAsStringAsync();

                    root = JsonSerializer.Deserialize<MangaListRoot>(nextString);
                    result.AddRange(root.MangaList.Select(x => new RankedManga { Ranking = x.Ranking, Manga = x.Manga }));

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
