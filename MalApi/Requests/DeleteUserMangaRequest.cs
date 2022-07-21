using System;
using System.Collections.Generic;
using System.Text;

namespace MalApi.Requests
{
    public class DeleteUserMangaRequest : HttpDeleteRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/manga/";

        public DeleteUserMangaRequest(int id)
        {
            PathParameters.Add(id);
            PathParameters.Add("my_list_status");
        }
    }
}
