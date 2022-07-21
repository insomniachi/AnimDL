using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MalApi;

public static class MalAuthHelper
{
    private static readonly string _codeChallenge;
    private static readonly string _codeVerifier;
    private static readonly string _authUrl = "https://myanimelist.net/v1/oauth2/";

    static MalAuthHelper()
    {
        _codeChallenge = _codeVerifier = GenerateCodeChallenge();
    }

    private static string GenerateCodeChallenge()
    {
        const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789-._~";
        return new string(Enumerable.Repeat(chars, 128)
          .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    private static async Task<OAuthToken> ReadAuthResults(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OAuthToken>(json);
        result.CreateAt = DateTime.UtcNow;
        return result;
    }

    public static string GetAuthUrl(string clientId)
    {
        var nvc = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["code_challenge"] = _codeChallenge,
            ["state"] = "abc"
        };

        StringBuilder q = new();
        foreach (var x in nvc)
        {
            q.Append(x.Key + "=" + x.Value);
            if (!x.Equals(nvc.Last()))
                q.Append('&');
        }

        return _authUrl + "authorize?" + q;
    }

    public static async Task<OAuthToken> DoAuth(string clientId, string code)
    {
        var nvc = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["code"] = code,
            ["code_verifier"] = _codeVerifier,
            ["grant_type"] = "authorization_code"
        };

        var req = new HttpRequestMessage(HttpMethod.Post, _authUrl + "token") { Content = new FormUrlEncodedContent(nvc) };
        using var client = new HttpClient();

        return await ReadAuthResults(await client.SendAsync(req).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public static async Task<OAuthToken> RefreshToken(string refreshToken, string clientId)
    {
        var nvc = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        var req = new HttpRequestMessage(HttpMethod.Post, _authUrl + "token") { Content = new FormUrlEncodedContent(nvc) };
        using var client = new HttpClient();

        return await ReadAuthResults(await client.SendAsync(req).ConfigureAwait(false)).ConfigureAwait(false);
    }

}

public class OAuthToken
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreateAt { get; set; }


    public bool IsExpired => (DateTime.UtcNow - CreateAt).Days > 31;
}
