using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part2.Attributes;
using ReflectionExamples.Part2.Services;
using ReflectionExamples.Part2.Services.Models;
using System.Net.Http.Headers;
using System.Reflection;

namespace ReflectionExamples.Part2
{
    public static class CommonServiceExtensions
    {
        public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");
            var githubClientConfig = githubConfig.Get<GithubSettings>();

            var githubAuthConfig = configuration.GetSection("GithubAuth");
            var githubAuthClientConfig = githubAuthConfig.Get<GithubAuthSettings>();

            services.Configure<GithubSettings>(githubConfig);
            services.Configure<GithubAuthSettings>(githubAuthConfig);

            var assembly = typeof(CommonServiceExtensions).Assembly;
            services.AddServices(assembly);

            services.AddTransient<GithubDelegatingHandler>();

            services.AddHttpClient<IGithubHttpClientFactory, GithubHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            }).AddHttpMessageHandler<GithubDelegatingHandler>();

            services.AddHttpClient<IGithubAuthHttpClientFactory, GithubAuthHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);
            });
        }

        private static void AddServices(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsNotStatic() && !t.IsInterface && t.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                var singleAttribute = Attribute.GetCustomAttribute(type, typeof(Singleton));

                foreach (var interfce in interfaces)
                {
                    if (singleAttribute == null)
                        services.AddTransient(interfce, type);
                    else
                        services.AddSingleton(interfce, type);
                }
            }
        }

        private static bool IsNotStatic(this Type t) => !t.IsAbstract && !t.IsSealed;
    }
}
