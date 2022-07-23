using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

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

    public interface IGetUserAnimeListRequest
    {
        IGetUserAnimeListRequest WithFields(params string[] fields);
        IGetUserAnimeListRequest WithOffset(int offset);
        IGetUserAnimeListRequest WithLimit(int limit);
        IGetUserAnimeListRequest SortBy(UserAnimeSort sort);
        IGetUserAnimeListRequest WithStatus(AnimeStatus status);
        Task<PagedAnime> Find();
    }

    public partial class AnimeEndPoint : IGetUserAnimeListRequest
    {
        public string User { get; set; }
        public AnimeStatus Status { get; set; } = AnimeStatus.None;
        public UserAnimeSort Sort { get; set; } = UserAnimeSort.UserScore;

        async Task<PagedAnime> IGetUserAnimeListRequest.Find()
        {
            var @params = new Dictionary<string, string>
            {
                ["limit"] = Limit.ToString(),
                ["offset"] = Offset.ToString(),
                ["sort"] = Sort.GetMalApiString(),
                ["fields"] = string.Join(",", Fields)
            };

            if(Status != AnimeStatus.None)
            {
                @params.Add("status", Status.GetMalApiString());
            }

            var url = QueryHelpers.AddQueryString($"https://api.myanimelist.net/v2/users/{User}/animelist", @params);
            var json = await Http.Client.GetStringAsync(url);
            var root = JsonSerializer.Deserialize<AnimeListRoot>(json);

            return new PagedAnime
            {
                Paging = root.Paging,
                Data = root.AnimeList.Select(x => x.Anime).ToList()
            };
        }

        public IGetUserAnimeListRequest OfUser(string user = @"me")
        {
            User = user;
            Limit = 500;
            MaxLimit = 1000;
            return this;
        }

        public IGetUserAnimeListRequest SortBy(UserAnimeSort sort)
        {
            Sort = sort;
            return this;
        }

        public AnimeEndPoint WithStatus(AnimeStatus status)
        {
            Status = status;
            return this;
        }

        IGetUserAnimeListRequest IGetUserAnimeListRequest.WithStatus(AnimeStatus status) => WithStatus(status);
        IGetUserAnimeListRequest IGetUserAnimeListRequest.WithFields(params string[] fields) => WithFields(fields);
        IGetUserAnimeListRequest IGetUserAnimeListRequest.WithLimit(int limit) => WithLimit(limit);
        IGetUserAnimeListRequest IGetUserAnimeListRequest.WithOffset(int offset) => WithOffset(offset);
    }

    public enum UserAnimeSort
    {
        UserScore,
        LastUpdated,
        Title,
        StartDate,
        Id,
    }

    public enum SeasonalAnimeSort
    {
        Score,
        NumberOfUsers
    }
}
