using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalApi.Requests
{
    public class UpdateAnimeUserStatusRequest : HttpPutRequest<UserAnimeStatus>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/";

        public UpdateAnimeUserStatusRequest(int id, AnimeStatus status = AnimeStatus.None, bool? isRewatching = null, int score = -1, int ep = -1, int priority = -1, int rewatchCount = -1, int rewatchValue = -1, string tags = "", string comments = "")
        {
            PathParameters.Add(id);
            PathParameters.Add("my_list_status");

            if (status != AnimeStatus.None)
            {
                Parameters.Add("status", status.GetMalApiString());
            }

            if (isRewatching != null)
            {
                Parameters.Add("is_rewatching", isRewatching);
            }

            if (score >= 0)
            {
                Parameters.Add("score", score);
            }

            if (ep >= 0)
            {
                Parameters.Add("num_watched_episodes", ep);
            }

            if (priority >= 0)
            {
                Parameters.Add("priority", priority);
            }

            if (rewatchCount >= 0)
            {
                Parameters.Add("num_times_rewatched", rewatchCount);
            }

            if (rewatchValue >= 0)
            {
                Parameters.Add("rewatch_value", rewatchValue);
            }

            if (string.IsNullOrEmpty(tags) == false)
            {
                Parameters.Add("tags", tags);
            }

            if (string.IsNullOrEmpty(comments) == false)
            {
                Parameters.Add("comments", comments);
            }

        }
    }


    public interface IUpdateRequest
    {
        IUpdateRequest WithStatus(AnimeStatus status);
        IUpdateRequest WithScore(int score);
        IUpdateRequest WithIsRewatching(bool rewatching);
        IUpdateRequest WithEpisodesWatched(int episodesWatched);
        IUpdateRequest WithPriority(int priority);
        IUpdateRequest WithRewatchCount(int rewatchCount);
        IUpdateRequest WithRewatchValue(int rewatchValue);
        IUpdateRequest WithTags(string tags);
        IUpdateRequest WithComments(string comments);
        Task<UserAnimeStatus> Publish();

    }

    public partial class GetAnimeRequestBuilder : IUpdateRequest
    {
        public bool? IsRewatching { get; set; }
        public int? Score { get; set; }
        public int? EpisodesWatched { get; set; }
        public int? Priority { get; set; }
        public int? RewatchCount { get; set; }
        public int? RewatchValue { get; set; }
        public string Tags { get; set; }
        public string Comments { get; set; }

        public async Task<UserAnimeStatus> Publish()
        {
            var url = $"https://api.myanimelist.net/v2/anime/{Id}/my_list_status";

            using var httpContent = new FormUrlEncodedContent(GetUpdateParams());
            httpContent.Headers.Clear();
            httpContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var response = await Http.Client.PutAsync(url, httpContent);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserAnimeStatus>(json);
        }

        public IUpdateRequest WithComments(string comments)
        {
            Comments = comments;
            return this;
        }

        public IUpdateRequest WithEpisodesWatched(int episodesWatched)
        {
            EpisodesWatched = episodesWatched;
            return this;
        }

        public IUpdateRequest WithIsRewatching(bool rewatching)
        {
            IsRewatching = rewatching;
            return this;
        }

        public IUpdateRequest WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        public IUpdateRequest WithRewatchCount(int rewatchCount)
        {
            RewatchCount = rewatchCount;
            return this;
        }

        public IUpdateRequest WithRewatchValue(int rewatchValue)
        {
            RewatchValue = rewatchValue;
            return this;
        }

        public IUpdateRequest WithTags(string tags)
        {
            Tags = tags;
            return this;
        }

        public IUpdateRequest WithScore(int score)
        {
            Score = score;
            return this;
        }

        IUpdateRequest IUpdateRequest.WithStatus(AnimeStatus status) => WithStatus(status);

        private IDictionary<string,string> GetUpdateParams()
        {
            var @params = new Dictionary<string, string>();

            if (Status is not AnimeStatus.None)
            {
                @params.Add("status", Status.GetMalApiString());
            }

            if (IsRewatching is { } isRewatching)
            {
                @params.Add("is_rewatching", isRewatching.ToString());
            }

            if (Score is { } score)
            {
                @params.Add("score", score.ToString());
            }

            if (EpisodesWatched is { } ep)
            {
                @params.Add("num_watched_episodes", ep.ToString());
            }

            if (Priority is { } priority)
            {
                @params.Add("priority", priority.ToString());
            }

            if (RewatchCount is { } rewatchCount)
            {
                @params.Add("num_times_rewatched", rewatchCount.ToString());
            }

            if (RewatchValue is { } rewatchValue)
            {
                @params.Add("rewatch_value", rewatchValue.ToString());
            }

            if (string.IsNullOrEmpty(Tags) == false)
            {
                @params.Add("tags", Tags);
            }

            if (string.IsNullOrEmpty(Comments) == false)
            {
                @params.Add("comments", Comments);
            }

            return @params;
        }
    }

}
