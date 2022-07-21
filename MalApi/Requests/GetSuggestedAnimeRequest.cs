using System;
using System.Collections.Generic;
using System.Text;

namespace MalApi.Requests
{
    public class GetSuggestedAnimeRequest : ListAnimeRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/suggestions";

        public GetSuggestedAnimeRequest(int limit = 25)
        {
            Count = limit;
        }
    }
}
