using HttpClientExamples.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part1
{
    public interface IGithubAuthClient
    {
        Task<string> GetTokenAsync();
    }

    public class GithubAuthClient : IGithubAuthClient
    {
        private readonly GithubAuthSettings _githubAuthSettings;

        public GithubAuthClient(IOptions<GithubAuthSettings> options)
        {
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

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubAuthSettings.BaseUrl),
            };

            var json = await httpClient.PostUrlFormAsync("/oauth/token", keyValuePairs);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            return tokenResponse.AccessToken;
        }
    }
}
