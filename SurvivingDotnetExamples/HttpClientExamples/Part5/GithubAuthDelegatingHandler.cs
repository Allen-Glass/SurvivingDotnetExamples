using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientExamples.Part5
{
    public class GithubAuthDelegatingHandler : DelegatingHandler
    {
        private readonly IGithubTokenCache _githubTokenCache;

        public GithubAuthDelegatingHandler(IGithubTokenCache githubTokenCache)
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
