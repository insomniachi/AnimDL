using AnimDL.Core.Api;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using AnimDL.Core.Helpers;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Extractors;

public class GogoPlayExtractor : IStreamExtractor
{
    private readonly Regex _encryptionRegex = new("data-value=\"(.+?)\"", RegexOptions.Compiled);
    private readonly Regex _keysRegex = new("(?:container|videocontent)-(\\d+)", RegexOptions.Compiled);

    public async Task<VideoStreamsForEpisode> Extract(string url)
    {
        var client = new HttpClient();
        var uri = new Uri(url);
        var id = QueryHelpers.ParseQuery(uri.Query)["id"].ToString();
        var nextHost = $"https://{uri.Host}/";
        var content = await client.GetStringAsync(url);

        var matches = _keysRegex.Matches(content);
        var encryptionKey = matches[0].Groups[1].Value;
        var iv = matches[1].Groups[1].Value;
        var decryptionKey = matches[2].Groups[1].Value;
        var encryptedData = _encryptionRegex.Match(content).Groups[1].Value;
        
        var decrypted = Decrypt(encryptedData, encryptionKey, iv);
        var newUrl = $"{decrypted}&id={Encrypt(id, encryptionKey, iv)}&alias={id}";
        var component = newUrl.Split("&", 2)[1];
        var ajaxUrl = $"{nextHost}encrypt-ajax.php?{component}";

        client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
        var response = await client.GetStringAsync(ajaxUrl);

        var data = JsonNode.Parse(response)!.AsObject()["data"]!.ToString();
        var decryptedData = Decrypt(data, decryptionKey, iv);
        var jsonData = JsonNode.Parse(decryptedData)!.AsObject();

        var streamForEp = new VideoStreamsForEpisode();

        foreach (var item in jsonData["source"]!.AsArray())
        {
            var quality = Regex.Match(item!["label"]!.ToString(), "(\\d+) P").Groups[1].Value;
            quality = string.IsNullOrEmpty(quality) ? "default" : quality;

            streamForEp.Qualities.Add(quality, new VideoStream
            {
                Quality = quality,
                Headers = new Dictionary<string, string> { ["referer"] = nextHost },
                Url = item!["file"]!.ToString()
            });
        }

        return streamForEp;
    }

    private static string Encrypt(string data, string key, string iv)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;
        var input = Encoding.UTF8.GetBytes(data);
        using var encryptor = aes.CreateEncryptor();
        var result = encryptor.TransformFinalBlock(input, 0, input.Length);
        return Convert.ToBase64String(result);
    }

    private static string Decrypt(string data,string key, string iv)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);
        aes.Padding = PaddingMode.PKCS7;
        var input = Convert.FromBase64String(data);
        using var decryptor = aes.CreateDecryptor();
        var result = decryptor.TransformFinalBlock(input, 0, input.Length);
        return Encoding.UTF8.GetString(result);
    }
}
