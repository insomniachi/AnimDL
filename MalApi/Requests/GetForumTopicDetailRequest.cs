using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MalApi.Models;

namespace MalApi.Requests
{
    public class GetForumTopicDetailRequest : HttpGetRequest<ForumTopicData>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/forum/topic/";

        public int Count { get; set; } = 1000;

        public GetForumTopicDetailRequest(int id)
        {
            PathParameters.Add(id);
        }

        protected async override Task<ForumTopicData> CreateResponse(string json)
        {
            List<ForumTopicPost> posts = new List<ForumTopicPost>();
            List<ForumTopicPoll> polls = new List<ForumTopicPoll>();
            string title = string.Empty;

            ForumTopicDataRoot root = JsonSerializer.Deserialize<ForumTopicDataRoot>(json);

            title = root.TopicData.Title;

            if (root.TopicData.Posts != null)
            {
                posts.AddRange(root.TopicData.Posts); 
            }

            if (root.TopicData.Poll != null)
            {
                polls.AddRange(root.TopicData.Poll);
            }

            while (string.IsNullOrEmpty(root.Paging.Next) == false)
            {
                var nextResponse = await httpClient.GetAsync(root.Paging.Next);
                string nextString = await nextResponse.Content.ReadAsStringAsync();

                root = JsonSerializer.Deserialize<ForumTopicDataRoot>(nextString);

                if (root.TopicData.Posts != null)
                {
                    posts.AddRange(root.TopicData.Posts);
                }

                if (root.TopicData.Poll != null)
                {
                    polls.AddRange(root.TopicData.Poll);
                }
            }

            return new ForumTopicData {
                Title = title,
                Posts = posts.ToArray(),
                Poll = polls.ToArray()
            };
        }
    }
}
