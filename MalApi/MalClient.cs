using System.Collections.Generic;
using System.Threading.Tasks;
using MalApi.Models;
using MalApi.Requests;

namespace MalApi
{
    public class MalClient
    {
       
        public MalClient(string accessToken)
        {
            HttpRequest.AccessToken = accessToken;
        }

        public async Task<Anime> GetAnimeAsync(int id)
        {
            var request = new GetAnimeDetailsRequest(id);

            return await request.GetAsync();
        }

        public async Task<List<Anime>> SearchAnimeAsync(string name, int limit = 25)
        {
            var request = new GetAnimeListRequest(name, limit);

            return await request.GetAsync();
        }

        public async Task<List<Anime>> GetSeasonalAnimeAsync(AnimeSeason season, int year, int limit = 25)
        {
            var request = new GetSeasonalAnimeRequest(season, year, limit);

            return await request.GetAsync();
        }

        public async Task<List<Anime>> GetUserAnimeAsync(AnimeStatus status = AnimeStatus.None)
        {
            var request = new GetUserAnimeListRequest(status);

            return await request.GetAsync();
        }


        public async Task<UserAnimeStatus> UpdateUserAnimeStatusAsync(int id, AnimeStatus status = AnimeStatus.None, bool? isRewatching = null, int score = -1, int ep = -1, int priority = -1, int rewatchCount = -1, int rewatchValue = -1, string tags = "", string comments = "")
        {
            var request = new UpdateAnimeUserStatusRequest(id, status, isRewatching, score, ep, priority, rewatchCount, rewatchValue, tags, comments);

            return await request.PutAsync();
        }

        public async Task<bool> DeleteUserAnimeAsync(int id)
        {
            var request = new DeleteUserAnimeRequest(id);

            return await request.DeleteAsync();
        }

        public async Task<List<RankedAnime>> GetRankedAnimeAsync(AnimeRankingType type, int limit = 25)
        {
            var request = new GetRankedAnimeRequest(type, limit);

            return await request.GetAsync();
        }

        public async Task<List<Anime>> GetSuggestedAnimeAsync(int limit = 25)
        {
            var request = new GetSuggestedAnimeRequest(limit);

            return await request.GetAsync();
        }

        public async Task<MalUser> GetUserMeAsync()
        {
            var request = new GetUserInformationRequest();

            return await request.GetAsync();
        }

        public async Task<List<ForumCategory>> GetForumBoardsAsync()
        {
            var request = new GetForumBoardsRequest();

            return await request.GetAsync();
        }

        public async Task<ForumTopicData> GetForumTopicDetailsAsync(int id)
        {
            var request = new GetForumTopicDetailRequest(id);

            return await request.GetAsync();
        }

        public async Task<List<ForumTopicDetails>> GetForumTopicsAsync(string querry, int boardId =1, int subBoardId = -1 , string topicUser = "", string user = "")
        {
            var request = new GetForumTopicsRequest(querry, boardId, subBoardId, topicUser, user);

            return await request.GetAsync();
        }

        public async Task<Manga> GetMangaAsync(int id)
        {
            var request = new GetMangaRequest(id);

            return await request.GetAsync();
        }

        public async Task<List<RankedManga>> GetRankedMangaAsync(MangaRankingType type = MangaRankingType.All, int count = 25)
        {
            var request = new GetRankedMangaRequest(type, count);

            return await request.GetAsync();
        }

        public async Task<UserMangaStatus> UpdateUserMangaStatusAsync(int id, MangaStatus status = MangaStatus.None, bool? isReReading = null, int score = -1, int volumesRead = -1,
                        int chaptersRead = -1, int priority = -1, int reReadCount = -1, int reReadValue = -1, string tags = "", string comments ="")
        {
            var request = new UpdateMangaUserStatusRequest(id, status, isReReading, score, volumesRead, chaptersRead, priority, reReadCount, reReadValue, tags, comments);

            return await request.PutAsync();
        }

        public async Task<bool> DeleteUserMangaAsync(int id)
        {
            var request = new DeleteUserMangaRequest(id);

            return await request.DeleteAsync();
        }

        public async Task<List<Manga>> GetUserMangaAsync(MangaStatus status = MangaStatus.None)
        {
            var request = new GetUserMangaListRequest(status);

            return await request.GetAsync();
        }
    }
}
