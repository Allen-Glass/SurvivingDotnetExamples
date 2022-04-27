using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part2.Services.Models;
using System.Net.Http.Headers;

//requires Microsoft.Extensions.Options.ConfigurationExtensions installed
namespace ReflectionExamples.Part2.Services
{
    public static class GithubServiceExtensions
    {
        public static void Add(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");
            var githubClientConfig = githubConfig.Get<GithubSettings>();

            var githubAuthConfig = configuration.GetSection("GithubAuth");
            var githubAuthClientConfig = githubAuthConfig.Get<GithubAuthSettings>();

            services.Configure<GithubAuthSettings>(githubAuthConfig);

            services.AddTransient<IGithubClient, GithubClient>();
            services.AddTransient<IGithubAuthClient, GithubAuthClient>();
            services.AddSingleton<IGithubTokenCache, GithubTokenCache>();
            services.AddTransient<IGithubService, GithubService>();
            services.AddTransient<GithubDelegatingHandler>();

            services.AddHttpClient<IGithubHttpClientFactory, GithubHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);

                //github documentation recommends using this Accept header
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }).AddHttpMessageHandler<GithubDelegatingHandler>();

            services.AddHttpClient<IGithubAuthHttpClientFactory, GithubAuthHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);
            });
        }
    }
}
