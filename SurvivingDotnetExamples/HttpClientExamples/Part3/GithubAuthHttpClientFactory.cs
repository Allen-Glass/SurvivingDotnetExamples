using System.Net.Http;

namespace HttpClientExamples.Part3
{
    public interface IGithubAuthHttpClientFactory
    {
        HttpClient Get();
    }

    public class GithubAuthHttpClientFactory : IGithubAuthHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public GithubAuthHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient Get()
        {
            return _httpClient;
        }
    }
}
