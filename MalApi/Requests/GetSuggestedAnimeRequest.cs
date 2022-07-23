using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

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

    public interface IRecommentedAnimeListRequest
    {
        IRecommentedAnimeListRequest WithLimit(int limits);
        IRecommentedAnimeListRequest WithOffset(int offset);
        IRecommentedAnimeListRequest WithFields(params string[] fields);
        Task<PagedAnime> Find();
    }

    public partial class AnimeEndPoint : IRecommentedAnimeListRequest
    {
        public async Task<PagedAnime> Find()
        {
            var url = QueryHelpers.AddQueryString("https://api.myanimelist.net/v2/anime/suggestions", new Dictionary<string, string>
            {
                ["limit"] = Limit.ToString(),
                ["offset"] = Offset.ToString(),
                ["fields"] = string.Join(",", Fields)
            });

            var json = await Http.Client.GetStringAsync(url);
            var room = JsonSerializer.Deserialize<AnimeListRoot>(json);

            return new PagedAnime
            {
                Paging = room.Paging,
                Data = room.AnimeList.Select(x => x.Anime).ToList()
            };
        }

        public IRecommentedAnimeListRequest SuggestedForMe()
        {
            Limit = MaxLimit = 100;
            return this;
        }

        IRecommentedAnimeListRequest IRecommentedAnimeListRequest.WithFields(params string[] fields) => WithFields(fields);
        IRecommentedAnimeListRequest IRecommentedAnimeListRequest.WithLimit(int limits) => WithLimit(limits);
        IRecommentedAnimeListRequest IRecommentedAnimeListRequest.WithOffset(int offset) => WithOffset(offset);
    }
}
