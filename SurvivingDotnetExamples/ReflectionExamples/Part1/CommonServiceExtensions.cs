using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part0.Services;
using ReflectionExamples.Part0.Services.Models;
using System.Net.Http.Headers;

namespace ReflectionExamples.Part1
{
    public static class CommonServiceExtensions
    {
        public static void AddServiceTypes(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");
            var githubClientConfig = githubConfig.Get<GithubSettings>();

            var githubAuthConfig = configuration.GetSection("GithubAuth");
            var githubAuthClientConfig = githubAuthConfig.Get<GithubAuthSettings>();

            services.Configure<GithubSettings>(githubConfig); 
            services.Configure<GithubAuthSettings>(githubAuthConfig);


            var assembly = typeof(CommonServiceExtensions).Assembly;
            var types = assembly.GetTypes().Where(t => t.IsNotStatic() && !t.IsInterface && t.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var interfce in interfaces)
                {
                    services.AddTransient(interfce, type);
                }
            }


            services.AddHttpClient<IGithubHttpClientFactory, GithubHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);

                //github documentation recommends using this Accept header
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }).AddHttpMessageHandler<GithubAuthDelegatingHandler>();

            services.AddHttpClient<IGithubAuthHttpClientFactory, GithubAuthHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);
            });
        }

        private static bool IsNotStatic(this Type t) => !t.IsAbstract && !t.IsSealed;
    }
}
