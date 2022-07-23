using MalApi.Requests;

namespace MalApi
{
    public enum AnimeStatus
    {
        Watching,
        Completed,
        OnHold,
        PlanToWatch,
        Dropped,
        None,
    }

    public static class EnumExtensions
    {
        public static string GetMalApiString(this AnimeStatus status)
        {
            return status switch
            {
                AnimeStatus.Completed => "completed",
                AnimeStatus.Dropped => "dropped",
                AnimeStatus.OnHold => "on_hold",
                AnimeStatus.PlanToWatch => "plan_to_watch",
                AnimeStatus.Watching => "watching",
                _ => string.Empty,
            };
        }

        public static string GetMalApiString(this MangaStatus status)
        {
            return status switch
            {
                MangaStatus.Completed => "completed",
                MangaStatus.Dropped => "dropped",
                MangaStatus.OnHold => "on_hold",
                MangaStatus.PlanToRead => "plan_to_read",
                MangaStatus.Reading => "reading",
                _ => string.Empty,
            };
        }

        public static string GetMalApiString(this Sort sort)
        {
            return sort switch
            {
                Sort.Score => "list_score",
                Sort.LastUpdated => "list_updated_at",
                Sort.Title => "anime_title",
                Sort.StartDate => "anime_start_date",
                Sort.Id => "anime_id ",
                _ => string.Empty
            };
        }

        public static AnimeStatus GetAnimeStatus(this string status)
        {
            return status switch
            {
                "completed" => AnimeStatus.Completed,
                "dropped" => AnimeStatus.Dropped,
                "on_hold" => AnimeStatus.OnHold,
                "plan_to_watch" => AnimeStatus.PlanToWatch,
                "watching" => AnimeStatus.Watching,
                _ => AnimeStatus.None,
            };
        }
    }
}
