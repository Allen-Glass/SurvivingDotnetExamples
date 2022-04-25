using ReflectionExamples.Part3.Services.Models;
using System.Text.Json;

namespace ReflectionExamples.Part3.Services
{
    public interface IGithubAuthClient
    {
        Task<TokenResponse> GetTokenAsync();
    }

    public class GithubAuthClient : IGithubAuthClient
    {
        private readonly IGithubAuthHttpClientFactory _githubAuthHttpClientFactory;
        private readonly GithubAuthSettings _githubAuthSettings;

        public GithubAuthClient(IGithubAuthHttpClientFactory githubAuthHttpClientFactory, GithubAuthSettings githubAuthSettings)
        {
            _githubAuthHttpClientFactory = githubAuthHttpClientFactory;
            _githubAuthSettings = githubAuthSettings;
        }

        public async Task<TokenResponse> GetTokenAsync()
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _githubAuthSettings.ClientId },
                { "client_secret", _githubAuthSettings.ClientSecret },
                { "audience", _githubAuthSettings.Audience }
            };

            using var httpClient = _githubAuthHttpClientFactory.Get();

            var json = await httpClient.PostUrlFormAsync("/oauth/token", keyValuePairs);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            return tokenResponse;
        }
    }
}
