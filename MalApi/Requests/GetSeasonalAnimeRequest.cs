using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace MalApi.Requests
{

    public class GetSeasonalAnimeRequest : ListAnimeRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime/season/";


        public GetSeasonalAnimeRequest(AnimeSeason season, int year, int limit = 25)
        {
            PathParameters.Add(year);
            PathParameters.Add(season.ToString().ToLower());
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");
            Count = limit;
        }


    }

    public interface ISeasonalAnimeListRequest
    {
        ISeasonalAnimeListRequest WithLimit(int limits);
        ISeasonalAnimeListRequest WithOffset(int offset);
        ISeasonalAnimeListRequest WithFields(params string[] fields);
        ISeasonalAnimeListRequest SortBy(SeasonalAnimeSort sort);
        Task<PagedAnime> Find();
    }

    public partial class AnimeEndPoint : ISeasonalAnimeListRequest
    {
        public int Year { get; set; }
        public AnimeSeason Season { get; set; }
        public SeasonalAnimeSort SeasonalAnimeSort { get; set; } = SeasonalAnimeSort.NumberOfUsers;

        async Task<PagedAnime> ISeasonalAnimeListRequest.Find()
{
            var url = QueryHelpers.AddQueryString($"https://api.myanimelist.net/v2/anime/season/{Year}/{Season.ToString().ToLower()}",
                new Dictionary<string, string>
                {
                    ["sort"] = SeasonalAnimeSort.GetMalApiString(),
                    ["limit"] = Limit.ToString(),
                    ["offset"] = Offset.ToString(),
                    ["fields"] = string.Join(",", Fields)
                });

            var json = await Http.Client.GetStringAsync(url);
            var root = JsonSerializer.Deserialize<AnimeListRoot>(json);

            return new PagedAnime
            {
                Paging = root.Paging,
                Data = root.AnimeList.Select(x => x.Anime).ToList()
            };
        }

        public ISeasonalAnimeListRequest OfSeason(AnimeSeason season, int year)
        {
            Limit = 100;
            MaxLimit = 500;
            Year = year;
            Season = season;
            return this;
        }

        public ISeasonalAnimeListRequest SortBy(SeasonalAnimeSort sort)
        {
            SeasonalAnimeSort = sort;
            return this;
        }

        ISeasonalAnimeListRequest ISeasonalAnimeListRequest.WithFields(params string[] fields) => WithFields(fields);
        ISeasonalAnimeListRequest ISeasonalAnimeListRequest.WithLimit(int limits) => WithLimit(limits);
        ISeasonalAnimeListRequest ISeasonalAnimeListRequest.WithOffset(int offset) => WithOffset(offset);
    }
}
