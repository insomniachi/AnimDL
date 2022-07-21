using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalApi.Requests
{
    public abstract class HttpRequest
    {
        protected HttpClient httpClient = new HttpClient();

        public List<object> PathParameters { get; } = new List<object>();

        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public abstract string BaseUrl { get; }

        public string GetRequestUri()
        {
            string uri = $"{Path.Combine(BaseUrl, string.Join("/", PathParameters))}";

            if(Parameters.Count > 0)
            {
                uri = $"{uri}?{string.Join("&", Parameters.Select(x => $"{x.Key}={x.Value}"))}";
            }

            return uri;
        }

        public HttpRequest()
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        }

        public static string AccessToken { get; set; }
    }

    public abstract class HttpGetRequest<T> : HttpRequest
    {
        public async Task<T> GetAsync()
        {
            HttpResponseMessage response = await httpClient.GetAsync(GetRequestUri());

            string content = await response.Content.ReadAsStringAsync();

            return await CreateResponse(content);
        }

        protected virtual Task<T> CreateResponse(string json)
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
    }

    public abstract class HttpPutRequest<T> : HttpRequest
    {
        public async Task<T> PutAsync()
        {
            using (var httpContent = new FormUrlEncodedContent(Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString())))
            {
                httpContent.Headers.Clear();
                httpContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                
                HttpResponseMessage response = await httpClient.PutAsync(GetRequestUri(), httpContent);
                
                string content = await response.Content.ReadAsStringAsync();
                
                return await CreateResponse(content);
            }
        }

        protected virtual Task<T> CreateResponse(string json)
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
    }

    public abstract class HttpDeleteRequest : HttpRequest
    {
        public async Task<bool> DeleteAsync()
        {
            HttpResponseMessage response = await httpClient.DeleteAsync(GetRequestUri());

            return response.IsSuccessStatusCode;
        }
    }
}
