using HttpClientExamples.Models;
using System;
using System.Threading.Tasks;

namespace HttpClientExamples.Part1
{
    public interface IGithubService
    {
        Task<Issue> GetIssueAsync(string id);

        Task<Issue> CreateIssueAsync(CreateIssue createIssue);
    }

    public class GithubService : IGithubService
    {
        private readonly IGithubClient _githubClient;

        public GithubService(IGithubClient githubClient)
        {
            _githubClient = githubClient;
        }

        public async Task<Issue> GetIssueAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var issue = await _githubClient.GetAsync<Issue>($"/repos/<owner>/<repo>/issues/{id}");

            return issue;
        }

        public async Task<Issue> CreateIssueAsync(CreateIssue createIssue)
        {
            if (createIssue == null)
                throw new ArgumentNullException(nameof(createIssue));

            var issue = await _githubClient.PostAsync<CreateIssue, Issue>(createIssue, "/repos/<owner>/<repo>/issues");

            return issue;
        }
    }
}
