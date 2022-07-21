using System;
using System.Collections.Generic;
using System.Text;

namespace MalApi.Requests
{
    public class GetUserMangaListRequest : ListMangaRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/users/@me/mangalist";

        public GetUserMangaListRequest(MangaStatus status)
        {
            if(status != MangaStatus.None)
            {
                Parameters.Add("status", status.GetMalApiString());
            }

            Count = 1000;

            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_volumes,num_chapters,authors{first_name,last_name},pictures,background,related_anime,related_manga,recommendations,serialization{name}");
        }
    }
}
