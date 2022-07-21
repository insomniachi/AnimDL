using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalApi.Requests
{

    public class GetRankedAnimeRequest : HttpGetRequest<List<RankedAnime>>
    {
        public int Count { get; set; }

        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/ranking";

        public GetRankedAnimeRequest(AnimeRankingType rankingType, int limit = 25)
        {
            Parameters.Add("ranking_type", rankingType.ToString().ToLower());
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");
            Count = limit;
        }

        protected override async Task<List<RankedAnime>> CreateResponse(string json)
        {
            List<RankedAnime> result = new List<RankedAnime>();

            AnimeListRoot root = JsonSerializer.Deserialize<AnimeListRoot>(json);
            result.AddRange(root.AnimeList.Select(x => new RankedAnime { Ranking = x.Ranking, Anime = x.Anime }));

            if (result.Count < Count)
            {
                while (string.IsNullOrEmpty(root.Paging.Next) == false)
                {
                    var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                    string nextString = await nextResponse.Content.ReadAsStringAsync();

                    root = JsonSerializer.Deserialize<AnimeListRoot>(nextString);
                    result.AddRange(root.AnimeList.Select(x => new RankedAnime { Ranking = x.Ranking, Anime = x.Anime }));

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
