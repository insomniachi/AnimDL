using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace MalApi.Requests
{
    public class GetAnimeListRequest : ListAnimeRequest
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/anime";

        public GetAnimeListRequest(string name, int limit = 25)
        {
            Parameters.Add("q", name);
            Parameters.Add("fields", "id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,nsfw,created_at,updated_at,media_type,status,genres,my_list_status,num_episodes,start_season,broadcast,source,average_episode_duration,rating,pictures,background,related_anime,related_manga,recommendations,studios,statistics");

            Count = limit;
        }
    }


    public interface IGetAnimeListRequestBuilder
    {
        IGetAnimeListRequest WithName(string name);
    }

    public interface IGetAnimeListRequest
    {
        IGetAnimeListRequest WithLimit(int limit);
        IGetAnimeListRequest WithFields(params string[] fields);
        IGetAnimeListRequest WithOffset(int offset);
        Task<PagedAnime> Find();
    }

    public partial class GetAnimeRequestBuilder : IGetAnimeListRequestBuilder, IGetAnimeListRequest
    {
        public string Name { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int MaxLimit { get; set; }

        public GetAnimeRequestBuilder WithLimit(int limit)
        {
            if(Limit > MaxLimit)
            {
                throw new ArgumentException($"argument greater than max value {MaxLimit}", nameof(limit));
            }

            Limit = limit;
            return this;
        }
        
        public GetAnimeRequestBuilder WithOffset(int offset)
        {
            Offset = offset;
            return this;
        }


        public IGetAnimeListRequest WithName(string name)
        {
            Limit = MaxLimit = 100;
            Name = name;
            return this;
        }

        IGetAnimeListRequest IGetAnimeListRequest.WithFields(params string[] fields) => WithFields(fields);
        IGetAnimeListRequest IGetAnimeListRequest.WithOffset(int offset) => WithOffset(offset);
        IGetAnimeListRequest IGetAnimeListRequest.WithLimit(int limit) => WithLimit(limit);
        async Task<PagedAnime> IGetAnimeListRequest.Find()
        {
            var url = QueryHelpers.AddQueryString($"https://api.myanimelist.net/v2/anime", new Dictionary<string, string>
            {
                ["q"] = Name,
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
    }

}
