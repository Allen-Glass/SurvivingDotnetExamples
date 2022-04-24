using System.Net.Http;

namespace HttpClientExamples.Part5
{
    public interface IGithubHttpClientFactory
    {
        HttpClient Get();
    }

    public class GithubHttpClientFactory : IGithubHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public GithubHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient Get()
        {
            return _httpClient;
        }
    }
}
