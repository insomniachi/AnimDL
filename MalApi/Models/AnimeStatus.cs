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

    public static class StatusExtensions
    {
        public static string GetMalApiString(this AnimeStatus status)
        {
            switch(status)
            {
                case AnimeStatus.Completed: return "completed";
                case AnimeStatus.Dropped: return "dropped";
                case AnimeStatus.None: return string.Empty;
                case AnimeStatus.OnHold: return "on_hold";
                case AnimeStatus.PlanToWatch: return "plan_to_watch";
                case AnimeStatus.Watching: return "watching";
                default: return string.Empty;
            }
        }

        public static string GetMalApiString(this MangaStatus status)
        {
            switch (status)
            {
                case MangaStatus.Completed: return "completed";
                case MangaStatus.Dropped: return "dropped";
                case MangaStatus.None: return string.Empty;
                case MangaStatus.OnHold: return "on_hold";
                case MangaStatus.PlanToRead: return "plan_to_read";
                case MangaStatus.Reading: return "reading";
                default: return string.Empty;
            }
        }

        public static AnimeStatus GetAnimeStatus(this string status)
        {
            switch (status)
            {
                case "completed": return AnimeStatus.Completed;
                case "dropped": return AnimeStatus.Dropped;
                case "on_hold": return AnimeStatus.OnHold;
                case "plan_to_watch": return AnimeStatus.PlanToWatch;
                case "watching": return AnimeStatus.Watching;
                default: return AnimeStatus.None;
            }
        }
    }
}
