using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part2
{
    public interface IGithubClient
    {
        Task<T> GetAsync<T>(string url);

        Task<TResult> PostAsync<T, TResult>(T model, string url);
    }

    public class GithubClient : IGithubClient
    {
        private readonly IGithubAuthClient _githubAuthClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public GithubClient(IHttpClientFactory httpClientFactory, IGithubAuthClient githubAuthClient)
        {
            _githubAuthClient = githubAuthClient;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var accessToken = await _githubAuthClient.GetTokenAsync();

            using var httpClient = _httpClientFactory.CreateClient("Github");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var json = await httpClient.GetJsonAsync(url);
            var model = JsonSerializer.Deserialize<T>(json); 
            return model;
        }

        public async Task<TResult> PostAsync<T, TResult>(T model, string url)
        {
            var accessToken = await _githubAuthClient.GetTokenAsync();

            var json = JsonSerializer.Serialize(model);
            using var httpClient = _httpClientFactory.CreateClient("Github");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var responseStringContent = await httpClient.PostJsonAsync(url, json);
            var outputModel = JsonSerializer.Deserialize<TResult>(json);

            return outputModel;
        }
    }
}
