using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System;

namespace AnimDL.Core.Extractors
{
    internal class RapidVideoExtractor : IStreamExtractor
    {
        const string POLLING_URL = "https://ws1.rapid-cloud.ru/socket.io/";
        private readonly Dictionary<string, string> _pollingParameters = new()
        {
            ["EIO"] = "4",
            ["transport"] = "polling"
        };
        private string _sid = "";

        public async Task<VideoStreamsForEpisode?> Extract(string url)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Referrer = new(Constants.Zoro);

            var cts = new CancellationTokenSource();
            var thread = new Thread(async () => await StartPolling(client, cts.Token));
            thread.Start();

            while(string.IsNullOrEmpty(_sid))
            {
                await Task.Delay(100);
            }

            cts.Cancel();

            var match = Regex.Match(url, "embed-6/([^?#&/.]+)");

            if(!match.Success)
            {
                return null;
            }

            var contentId = match.Groups[1].Value;

            var recaptchaResonse = await client.BypassRecaptcha(url);

            if(recaptchaResonse is null)
            {
                return null;
            }

            var ajaxUrl = QueryHelpers.AddQueryString($"https://{new Uri(url).Host}/ajax/embed-6/getSources", new Dictionary<string, string>
            {
                ["id"] = contentId,
                ["_token"] = recaptchaResonse.Value.Token,
                ["_number"] = recaptchaResonse.Value.Number
            });

            var ajaxResponse = await client.GetStringAsync(ajaxUrl);

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

            (_, var result) = await Poll(client);

            var jsonObject = JsonNode.Parse(result)!.AsObject();
            var pingInterval = jsonObject["pingInterval"]!.ToString() ?? "1000";
            var pollingSid = $"{jsonObject["sid"]}";

            (_, var clientCheck) = await Poll(client, new Dictionary<string, string> { ["sid"] = pollingSid }, "40");

            if (clientCheck != "ok")
            {
                throw new Exception($"Websocket server has returned a faulty value: {clientCheck}");
            }

            (_, result) = await Poll(client, new Dictionary<string, string> { ["sid"] = pollingSid });

            jsonObject = JsonNode.Parse(result)!.AsObject();
            _sid = $"{jsonObject["sid"]}";

            //while (!token.IsCancellationRequested)
            //{
            //    (_, result) = await Poll(client, new Dictionary<string, string> { ["sid"] = pollingSid });

            //    if (result != "2")
            //    {
            //        throw new Exception($"Websocket server has returned a faulty value: {result}");
            //    }

            //    (_, result) = await Poll(client, new Dictionary<string, string> { ["sid"] = pollingSid }, "3");

            //    if (result != "3")
            //    {
            //        throw new Exception($"Websocket server has returned a faulty value: {result}");
            //    }
            //}
        }


        private async Task<(int id, string json)> Poll(HttpClient client, Dictionary<string,string>? @params = null, string? data = null)
        {
            if(@params is not null)
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
                response = await client.GetStringAsync(url);
            }
            else
            {
                var postResponse = await client.PostAsync(url, new StringContent(data));
                response = await postResponse.Content.ReadAsStringAsync();
            }

            return ParseResponse(response);

        }

        private (int id,string json) ParseResponse(string text)
        {
            var match = Regex.Match(text, "(\\d+)(.+)");

            if(match.Success)
            {
                return (int.Parse(match.Groups[1].Value), match.Groups[2].Value);
            }

            return (-1, text);
        }
    }
}
