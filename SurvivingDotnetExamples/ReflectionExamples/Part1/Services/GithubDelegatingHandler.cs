using System.Net.Http.Headers;

namespace ReflectionExamples.Part1.Services
{
    public class GithubDelegatingHandler : DelegatingHandler
    {
        private readonly IGithubTokenCache _githubTokenCache;

        public GithubDelegatingHandler(IGithubTokenCache githubTokenCache)
        {
            _githubTokenCache = githubTokenCache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _githubTokenCache.GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
