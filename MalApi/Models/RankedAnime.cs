namespace MalApi
{
    public class RankedAnime
    {
        public Anime Anime { get; set; }

        public Ranking Ranking { get; set; }

        public override string ToString()
        {
            return $"({Ranking.CurrentRank}) {Anime}";
        }
    }
}
