using System.Text.Json;

namespace ReflectionExamples.Part3.Services
{
    public interface IGithubClient
    {
        Task<T> GetAsync<T>(string url);

        Task<TResult> PostAsync<T, TResult>(T model, string url);
    }

    public class GithubClient : IGithubClient
    {
        private readonly IGithubHttpClientFactory _githubHttpClientFactory;

        public GithubClient(IGithubHttpClientFactory githubHttpClientFactory)
        {
            _githubHttpClientFactory = githubHttpClientFactory;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            using var httpClient = _githubHttpClientFactory.Get();

            var json = await httpClient.GetJsonAsync(url);
            var model = JsonSerializer.Deserialize<T>(json); 
            return model;
        }

        public async Task<TResult> PostAsync<T, TResult>(T model, string url)
        {
            var json = JsonSerializer.Serialize(model);
            using var httpClient = _githubHttpClientFactory.Get();

            var responseStringContent = await httpClient.PostJsonAsync(url, json);
            var outputModel = JsonSerializer.Deserialize<TResult>(json);

            return outputModel;
        }
    }
}
