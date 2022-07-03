namespace AnimDL.Extractors
{
    public interface IProvider
    {
        //IEnumerable<KeyValuePair<string, string>> Fetch(string url);
        //IEnumerable<KeyValuePair<string, string>> FetchEpisodes(string url, int start, int end);
        //string FetchEpisode(string url, int ep);
        int GetEpisodeCount(string url);
    }
}