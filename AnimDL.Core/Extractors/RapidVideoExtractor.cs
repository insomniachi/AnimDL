using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AnimDL.Core.Extractors
{
    internal partial class RapidVideoExtractor : IStreamExtractor
    {
        const string POLLING_URL = "https://ws1.rapid-cloud.ru/socket.io/";
        private readonly Dictionary<string, string> _pollingParameters = new()
        {
            ["EIO"] = "4",
            ["transport"] = "polling"
        };
        private readonly ILogger<RapidVideoExtractor> _logger;
        private readonly HttpClient _client;
        private string _sid = "";


        [GeneratedRegex("embed-6/([^?#&/.]+)")]
        private static partial Regex ContentIdRegex();

        [GeneratedRegex("(\\d+)(.+)")]
        private static partial Regex ClientCheckRegex();

        public RapidVideoExtractor(ILogger<RapidVideoExtractor> logger)
        {
            _logger = logger;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            _client.DefaultRequestHeaders.Referrer = new(Constants.Zoro);
        }

        public async Task<VideoStreamsForEpisode?> Extract(string url)
        {
            var cts = new CancellationTokenSource();
            var thread = new Thread(async () =>
            {
                try
                {
                    await StartPolling(_client, cts.Token);
                }
                catch { }
            });
            thread.Start();

            while (string.IsNullOrEmpty(_sid))
            {
                await Task.Delay(100);
            }

            cts.Cancel();

            var match = ContentIdRegex().Match(url);

            if (!match.Success)
            {
                return null;
            }

            var contentId = match.Groups[1].Value;

            var recaptchaResonse = await _client.BypassRecaptcha(url);

            if (recaptchaResonse is null)
            {
                return null;
            }


            var ajaxResponse = await _client.GetStringAsync($"https://{new Uri(url).Host}/ajax/embed-6/getSources", parameters: new()
            {
                ["id"] = contentId,
                ["_token"] = recaptchaResonse.Value.Token,
                ["_number"] = recaptchaResonse.Value.Number
            });

            var json = JsonNode.Parse(ajaxResponse);

            var streamForEp = new VideoStreamsForEpisode();
            foreach (var item in json!["sources"]!.AsArray())
            {
                var stream = new VideoStream
                {
                    Url = $"{item!["file"]}"
                };
                stream.Headers.Add("SID", _sid);
                stream.Quality = "default";

                streamForEp.Qualities.Add("default", stream);
            }

            return streamForEp;
        }

        private async Task StartPolling(HttpClient client, CancellationToken token)
        {

            (_, var result) = await Poll();

            var jsonObject = JsonNode.Parse(result)!.AsObject();
            var pingInterval = jsonObject["pingInterval"]!.ToString() ?? "1000";
            var pollingSid = $"{jsonObject["sid"]}";

            (_, var clientCheck) = await Poll(new Dictionary<string, string> { ["sid"] = pollingSid }, "40");

            if (clientCheck != "ok")
            {
                throw new Exception($"Websocket server has returned a faulty value: {clientCheck}");
            }

            (_, result) = await Poll(new Dictionary<string, string> { ["sid"] = pollingSid });

            jsonObject = JsonNode.Parse(result)!.AsObject();
            _sid = $"{jsonObject["sid"]}";

            while (!token.IsCancellationRequested)
            {
                (_, result) = await Poll(new Dictionary<string, string> { ["sid"] = pollingSid });

                if (result != "2")
                {
                    _logger.LogWarning("Websocket server has returned a faulty value: {Result}", result);
                    continue;
                }

                (_, result) = await Poll(new Dictionary<string, string> { ["sid"] = pollingSid }, "3");

                if (result != "3")
                {
                    _logger.LogWarning("Websocket server has returned a faulty value: {Result}", result);
                }
            }
        }


        private async Task<(int id, string json)> Poll(Dictionary<string, string>? @params = null, string? data = null)
        {
            if (@params is not null)
            {
                foreach (var item in @params.Where(x => !_pollingParameters.ContainsKey(x.Key)))
                {
                    _pollingParameters.Add(item.Key, item.Value);
                }
            }

            var url = QueryHelpers.AddQueryString(POLLING_URL, _pollingParameters);
            string response;
            if (data is null)
            {
                response = await _client.GetStringAsync(url);
            }
            else
            {
                var postResponse = await _client.PostAsync(url, new StringContent(data));
                response = await postResponse.Content.ReadAsStringAsync();
            }

            return ParseResponse(response);

        }

        private static (int id, string json) ParseResponse(string text)
        {
            var match = ClientCheckRegex().Match(text);

            if (match.Success)
            {
                return (int.Parse(match.Groups[1].Value), match.Groups[2].Value);
            }

            return (-1, text);
        }
    }
}
