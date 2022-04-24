using HttpClientExamples.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part3
{
    public interface IGithubAuthClient
    {
        Task<string> GetTokenAsync();
    }

    public class GithubAuthClient : IGithubAuthClient
    {
        private readonly IGithubAuthHttpClientFactory _githubAuthHttpClientFactory;
        private readonly GithubAuthSettings _githubAuthSettings;

        public GithubAuthClient(IGithubAuthHttpClientFactory githubAuthHttpClientFactory, IOptions<GithubAuthSettings> options)
        {
            _githubAuthHttpClientFactory = githubAuthHttpClientFactory;
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

            using var httpClient = _githubAuthHttpClientFactory.Get();

            var json = await httpClient.PostUrlFormAsync("/oauth/token", keyValuePairs);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            return tokenResponse.AccessToken;
        }
    }
}
