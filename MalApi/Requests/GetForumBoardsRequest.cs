using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MalApi.Models;

namespace MalApi.Requests
{
    public class GetForumBoardsRequest : HttpGetRequest<List<ForumCategory>>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/forum/boards";

        protected override Task<List<ForumCategory>> CreateResponse(string json)
        {
            ForumCategoryRoot root = JsonSerializer.Deserialize<ForumCategoryRoot>(json);

            return Task.FromResult(root.Categories.ToList());
        }
    }
}
