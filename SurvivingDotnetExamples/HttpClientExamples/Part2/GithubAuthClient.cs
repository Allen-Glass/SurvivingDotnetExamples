using HttpClientExamples.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part2
{
    public interface IGithubAuthClient
    {
        Task<string> GetTokenAsync();
    }

    public class GithubAuthClient : IGithubAuthClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GithubAuthSettings _githubAuthSettings;

        public GithubAuthClient(IHttpClientFactory httpClientFactory, IOptions<GithubAuthSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _githubAuthSettings = options.Value;
        }

        public async Task<string> GetTokenAsync()
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _githubAuthSettings.ClientId },
                { "client_secret", _githubAuthSettings.ClientSecret },
                { "audience", _githubAuthSettings.Audience }
            };

            using var httpClient = _httpClientFactory.CreateClient("GithubAuth");

            var json = await httpClient.PostUrlFormAsync("/oauth/token", keyValuePairs);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            return tokenResponse.AccessToken;
        }
    }
}
