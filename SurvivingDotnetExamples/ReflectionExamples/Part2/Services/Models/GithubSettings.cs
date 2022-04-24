using ReflectionExamples.Part2.Attributes;

namespace ReflectionExamples.Part2.Services.Models
{
    [Configuration("Github")]
    public class GithubSettings
    {
        public string BaseUrl { get; set; }
    }
}
