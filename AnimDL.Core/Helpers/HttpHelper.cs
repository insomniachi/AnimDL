namespace AnimDL.Core.Helpers;

internal static class HttpHelper
{
    internal static async Task<string> PostFormUrlEncoded(string url, IEnumerable<KeyValuePair<string, string>> postData)
    {
        using var client = new HttpClient();
        return await PostFormUrlEncoded(client, url, postData);
    }

    internal static async Task<string> PostFormUrlEncoded(HttpClient client, string url, IEnumerable<KeyValuePair<string, string>> postData)
    {
        using var content = new FormUrlEncodedContent(postData);
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        HttpResponseMessage response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}
