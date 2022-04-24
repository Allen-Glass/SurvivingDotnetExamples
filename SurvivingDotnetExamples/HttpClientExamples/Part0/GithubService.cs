using HttpClientExamples.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpClientExamples.Part0
{
    public interface IGithubService
    {
        Task<Issue> GetIssueAsync(string id);

        Task<Issue> CreateIssueAsync(CreateIssue createIssue);
    }

    public class GithubService : IGithubService
    {
        private readonly GithubSettings _githubSettings;

        public GithubService(IOptions<GithubSettings> options)
        {
            _githubSettings = options.Value;
        }

        public async Task<Issue> GetIssueAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));


            var accessToken = await GetTokenAsync();

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubSettings.BaseUrl),
            };

            //github documentation recommends using this Accept header
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var httpResponseMessage = await httpClient.GetAsync($"/repos/<owner>/<repo>/issues/{id}");
            var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new HttpRequestException($"There was an issue sending a request. Status Code: {httpResponseMessage.StatusCode}" +
                    $" See exception details: {responseStringContent}");

            var issue = JsonSerializer.Deserialize<Issue>(responseStringContent);
            return issue;
        }

        public async Task<Issue> CreateIssueAsync(CreateIssue createIssue)
        {
            if (createIssue == null)
                throw new ArgumentNullException(nameof(createIssue));

            var accessToken = await GetTokenAsync();
            var json = JsonSerializer.Serialize(createIssue);
            var httpContent = new StringContent(json);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubSettings.BaseUrl)
            };

            //github documentation recommends using this Accept header
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var httpResponseMessage = await httpClient.PostAsync("/repos/<owner>/<repo>/issues", httpContent);
            var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new HttpRequestException($"There was an issue sending a request. Status Code: {httpResponseMessage.StatusCode}" +
                    $" See exception details: {responseStringContent}");

            var outputModel = JsonSerializer.Deserialize<Issue>(responseStringContent);

            return outputModel;
        }

        private async Task<string> GetTokenAsync()
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _githubSettings.ClientId },
                { "client_secret", _githubSettings.ClientSecret },
                { "audience", _githubSettings.Audience }
            };

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_githubSettings.BaseUrl),
            };

            var urlEncodedContent = new FormUrlEncodedContent(keyValuePairs);

            var httpResponseMessage = await httpClient.PostAsync("/oauth/token", urlEncodedContent);
            var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new HttpRequestException($"There was an issue sending a request. Status Code: {httpResponseMessage.StatusCode}" +
                    $" See exception details: {responseStringContent}");

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseStringContent);


            return tokenResponse.AccessToken;
        }
    }
}
