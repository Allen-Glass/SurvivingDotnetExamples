using ReflectionExamples.Part4.Attributes;

namespace ReflectionExamples.Part4.Services.Models
{
    [Configuration]
    public class GithubAuthSettings
    {
        public string BaseUrl { get; set; }

        public string Audience { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
