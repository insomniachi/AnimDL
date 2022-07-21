using System;
using System.Text;

namespace MalApi.Requests
{

    public class GetSeasonalAnimeRequest : ListAnimeRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/season/";


        public GetSeasonalAnimeRequest(AnimeSeason season, int year, int limit = 25)
        {
            PathParameters.Add(year);
            PathParameters.Add(season.ToString().ToLower());
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");
            Count = limit;
        }


    }
}
