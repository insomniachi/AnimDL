using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace MalApi.Requests
{
    public class GetAnimeDetailsRequest : HttpGetRequest<Anime>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/";

        public GetAnimeDetailsRequest(int id)
        {
            PathParameters.Add(id);
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");
        }
    }

    public interface IAnimeEndPoint
    {
        IGetAnimeRequest WithId(long id);
        IGetAnimeListRequest WithName(string name);
        IGetUserAnimeListRequest OfUser(string user = "@me");
        ISeasonalAnimeListRequest OfSeason(AnimeSeason season, int year);
        IRecommentedAnimeListRequest SuggestedForMe();
        IRankedAnimeListRequest Top(AnimeRankingType rankingType);
    }

    public interface IGetAnimeRequest
    {
        IGetAnimeRequest WithFields(params string[] fields);
        IUpdateRequest UpdateStatus();
        Task<Anime> Find();
    }

    public partial class AnimeEndPoint : IAnimeEndPoint, IGetAnimeRequest
    {
        public long Id { get; set; }
        public List<string> Fields { get; set; } = new() { FieldName.Id, FieldName.Title, FieldName.MainPicture };

        public IGetAnimeRequest WithId(long id)
        {
            Id = id;
            return this;
        }

        public IUpdateRequest UpdateStatus() => this;

        IGetAnimeRequest IGetAnimeRequest.WithFields(params string[] fields)
        {
            return WithFields(fields);
        }

        private AnimeEndPoint WithFields(params string[] fields)
        {
            foreach (var item in fields)
            {
                if (!Fields.Contains(item))
                {
                    Fields.Add(item);
                }
            }
            return this;
        }

        async Task<Anime> IGetAnimeRequest.Find()
        {
            var url = QueryHelpers.AddQueryString($"https://api.myanimelist.net/v2/anime/{Id}", new Dictionary<string,string>
            {
                ["fields"] = string.Join(",", Fields)
            });

            var json = await Http.Client.GetStringAsync(url);
            return JsonSerializer.Deserialize<Anime>(json);
        }
    }

    public class FieldName
    {
        public static readonly string Id = "id";
        public static readonly string Title = "title";
        public static readonly string MainPicture = "main_picture";
        public static readonly string UserStatus = "my_list_status";
    }
}
