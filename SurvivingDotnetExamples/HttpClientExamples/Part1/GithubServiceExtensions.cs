using HttpClientExamples.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

//requires Microsoft.Extensions.Options.ConfigurationExtensions installed
namespace HttpClientExamples.Part1
{
    public static class GithubServiceExtensions
    {
        public static void Add(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");
            var githubAuthConfig = configuration.GetSection("GithubAuth");

            services.Configure<GithubSettings>(githubConfig);
            services.Configure<GithubAuthSettings>(githubAuthConfig);

            services.AddTransient<IGithubClient, GithubClient>();
            services.AddTransient<IGithubAuthClient, GithubAuthClient>();
            services.AddTransient<IGithubService, GithubService>();
        }
    }
}
