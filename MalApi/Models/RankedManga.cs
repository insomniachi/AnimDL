namespace MalApi
{
    public class RankedManga
    {
        public Manga Manga { get; set; }

        public Ranking Ranking { get; set; }

        public override string ToString()
        {
            return $"({Ranking.CurrentRank}) {Manga}";
        }
    }
}
