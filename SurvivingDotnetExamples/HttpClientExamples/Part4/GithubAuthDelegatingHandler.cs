using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientExamples.Part4
{
    public class GithubAuthDelegatingHandler : DelegatingHandler
    {
        private readonly IGithubAuthClient _githubAuthClient;

        public GithubAuthDelegatingHandler(IGithubAuthClient githubAuthClient)
        {
            _githubAuthClient = githubAuthClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _githubAuthClient.GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
