﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MalApi.Requests
{
    public class GetUserAnimeListRequest : ListAnimeRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/users/@me/animelist";

        public GetUserAnimeListRequest(AnimeStatus status)
        {
            if (status != AnimeStatus.None)
            {
                Parameters.Add("status", status.GetMalApiString()); 
            }

            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");

            Count = 1000;
        }
    }
}
