namespace MalApi.Requests
{
    public class UpdateAnimeUserStatusRequest : HttpPutRequest<UserAnimeStatus>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/";

        public UpdateAnimeUserStatusRequest(int id, AnimeStatus status = AnimeStatus.None, bool? isRewatching = null, int score = -1, int ep = -1, int priority = -1, int rewatchCount = -1, int rewatchValue = -1, string tags ="", string comments ="")
        {
            PathParameters.Add(id);
            PathParameters.Add("my_list_status");

            if(status != AnimeStatus.None)
            {
                Parameters.Add("status", status.GetMalApiString());
            }

            if(isRewatching != null)
            {
                Parameters.Add("is_rewatching", isRewatching);
            }

            if (score >= 0)
            {
                Parameters.Add("score", score);
            }

            if(ep >=0)
            {
                Parameters.Add("num_watched_episodes", ep);
            }

            if(priority >= 0)
            {
                Parameters.Add("priority", priority);
            }

            if(rewatchCount >=0)
            {
                Parameters.Add("num_times_rewatched", rewatchCount);
            }

            if(rewatchValue >=0)
            {
                Parameters.Add("rewatch_value", rewatchValue);
            }

            if(string.IsNullOrEmpty(tags) == false)
            {
                Parameters.Add("tags", tags);
            }

            if (string.IsNullOrEmpty(comments) == false)
            {
                Parameters.Add("comments", comments);
            }

        }
    }
}
