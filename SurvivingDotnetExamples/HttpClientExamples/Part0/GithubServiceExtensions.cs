using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

//requires Microsoft.Extensions.Options.ConfigurationExtensions installed
namespace HttpClientExamples.Part0
{
    public static class GithubServiceExtensions
    {
        public static void Add(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");

            services.Configure<GithubSettings>(githubConfig);

            services.AddTransient<IGithubService, GithubService>();
        }
    }
}
