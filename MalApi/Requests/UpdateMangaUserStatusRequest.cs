using System;
using System.Collections.Generic;
using System.Text;

namespace MalApi.Requests
{
    public class UpdateMangaUserStatusRequest : HttpPutRequest<UserMangaStatus>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/manga/";
        
        public UpdateMangaUserStatusRequest(int id, MangaStatus status = MangaStatus.None, bool? isRereading = null, int score = -1, int volumesRead = -1, int chapterRead =-1, int priority = -1, int rereadCount = -1, int rereadValue = -1, string tags = "", string comments = "")
        {
            PathParameters.Add(id);
            PathParameters.Add("my_list_status");

            if(isRereading != null)
            {
                Parameters.Add("is_rereading", isRereading);
            }

            if(status != MangaStatus.None)
            {
                Parameters.Add("status", status.GetMalApiString());
            }

            if(score >= 0)
            {
                Parameters.Add("score", score);
            }

            if(volumesRead >=0)
            {
                Parameters.Add("num_volumes_read", volumesRead);
            }

            if(chapterRead >=0)
            {
                Parameters.Add("num_chapters_read", chapterRead);
            }

            if(priority >= 0)
            {
                Parameters.Add("priority", priority);
            }

            if(rereadCount >=0)
            {
                Parameters.Add("num_times_reread", rereadCount);
            }

            if(rereadValue >=0)
            {
                Parameters.Add("reread_value", rereadValue);
            }

            if(string.IsNullOrEmpty(tags) == false)
            {
                Parameters.Add("tags", tags);
            }

            if(string.IsNullOrEmpty(comments) == false)
            {
                Parameters.Add("comments", comments);
            }
        }
    }
}
