using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MalApi.Models;

namespace MalApi.Requests
{
    public class GetForumTopicsRequest : HttpGetRequest<List<ForumTopicDetails>>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/forum/topics";

        public int Count { get; set; }

        public GetForumTopicsRequest(string q,int boardId = -1, int subBoardId = -1, string topicUsername = "", string username ="", int count = 1000)
        {
            if (boardId >= 0)
            {
                Parameters.Add("board_id", boardId);
            }

            if (subBoardId >= 0)
            {
                Parameters.Add("subboard_id", subBoardId);
            }
            
            if (string.IsNullOrEmpty(q) == false)
            {
                Parameters.Add("q", q); 
            }

            if (string.IsNullOrEmpty(topicUsername) == false)
            {
                Parameters.Add("topic_user_name", topicUsername);
            }

            if (string.IsNullOrEmpty(topicUsername) == false)
            {
                Parameters.Add("user_name", username);
            }

            Count = count;
        }

        protected async override Task<List<ForumTopicDetails>> CreateResponse(string json)
        {
            List<ForumTopicDetails> result = new List<ForumTopicDetails>();

            ForumTopicDetailRoot root = JsonSerializer.Deserialize<ForumTopicDetailRoot>(json);
            result.AddRange(root.Details);

            if (result.Count < Count)
            {

                while (string.IsNullOrEmpty(root.Paging.Next) == false)
                {
                    var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                    string nextString = await nextResponse.Content.ReadAsStringAsync();

                    root = JsonSerializer.Deserialize<ForumTopicDetailRoot>(nextString);
                    result.AddRange(root.Details);

                    if(result.Count >= Count)
                    {
                        break;
                    }
                }
            }

            return result.Take(Count).ToList();
        }
    }
}
