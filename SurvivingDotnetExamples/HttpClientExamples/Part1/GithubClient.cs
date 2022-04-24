using HttpClientExamples.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part1
{
    public interface IGithubClient
    {
        Task<T> GetAsync<T>(string url);

        Task<TResult> PostAsync<T, TResult>(T model, string url);
    }

    public class GithubClient : IGithubClient
    {
        private readonly IGithubAuthClient _githubAuthClient;
        private readonly GithubSettings _githubSettings;

        public GithubClient(IGithubAuthClient githubAuthClient, IOptions<GithubSettings> options)
        {
            _githubAuthClient = githubAuthClient;
            _githubSettings = options.Value;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var accessToken = await _githubAuthClient.GetTokenAsync();

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubSettings.BaseUrl),
            };

            //github documentation recommends using this Accept header
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var json = await httpClient.GetJsonAsync(url);
            var model = JsonSerializer.Deserialize<T>(json); 
            return model;
        }

        public async Task<TResult> PostAsync<T, TResult>(T model, string url)
        {
            var accessToken = await _githubAuthClient.GetTokenAsync();
            var json = JsonSerializer.Serialize(model);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubSettings.BaseUrl)
            };

            //github documentation recommends using this Accept header
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var responseStringContent = await httpClient.PostJsonAsync(url, json);
            var outputModel = JsonSerializer.Deserialize<TResult>(json);

            return outputModel;
        }
    }
}
