using AnimDL.Core.Api;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using AnimDL.Core.Helpers;

namespace AnimDL.Core.Extractors;

public class GogoPlayExtractor : IStreamExtractor
{
    private readonly Regex _encryptionRegex = new("data-value=\"(.+?)\"", RegexOptions.Compiled);
    private readonly Regex _keysRegex = new("(?:container|videocontent)-(\\d+)", RegexOptions.Compiled);

    public async Task<HlsStreams> Extract(string url)
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
        var streamUrl = $"{nextHost}encrypt-ajax.php?{component}";

        client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
        var response = await client.GetStringAsync(streamUrl);

        return new();
    }

    private string Encrypt(string data, string key, string iv)
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

    private string Decrypt(string data,string key, string iv)
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

    private static byte[] Pad(string data)
    {
        var len = data.Length;
        var padLenght = 16 - len % 16;
        var datahex = Convert.FromBase64String(data);
        var array = new byte[data.Length + padLenght];
        datahex.CopyTo(array, 0);
        for (int i = data.Length; i < array.Length; i++)
        {
            array[i] = (byte)padLenght;
        }

        return array;
    }

    static bool IsUnwanted(char b)
    {
        return b switch
        {
            '\x00' => true,
            '\x01' => true,
            '\x02' => true,
            '\x03' => true,
            '\x04' => true,
            '\x05' => true,
            '\x06' => true,
            '\x07' => true,
            '\x08' => true,
            '\t' => true,
            '\n' => true,
            '\x0b' => true,
            '\x0c' => true,
            '\r' => true,
            '\x0e' => true,
            '\x0f' => true,
            '\x10' => true,
            _ => false
        };
    }
}
