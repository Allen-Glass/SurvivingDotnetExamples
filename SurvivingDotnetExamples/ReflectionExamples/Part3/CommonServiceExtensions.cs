﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part3.Attributes;
using ReflectionExamples.Part3.Services;
using ReflectionExamples.Part3.Services.Models;
using System.Net.Http.Headers;
using System.Reflection;

namespace ReflectionExamples.Part3
{
    public static class CommonServiceExtensions
    {
        public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var githubConfig = configuration.GetSection("Github");
            var githubClientConfig = githubConfig.Get<GithubSettings>();

            var githubAuthConfig = configuration.GetSection("GithubAuth");
            var githubAuthClientConfig = githubAuthConfig.Get<GithubAuthSettings>();

            var assembly = typeof(CommonServiceExtensions).Assembly;

            services.AddServices(assembly);
            services.AddConfiguration(configuration, assembly);
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

        private static void AddServices(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsNotStatic() && !t.IsInterface && t.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                var singleAttribute = Attribute.GetCustomAttribute(type, typeof(Singleton));
                var config = Attribute.GetCustomAttribute(type, typeof(Configuration)) as Configuration;

                foreach (var interfce in interfaces)
                {
                    if (singleAttribute == null)
                        services.AddTransient(interfce, type);
                    else
                        services.AddSingleton(interfce, type);
                }
            }
        }

        private static void AddConfiguration(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(type => Attribute.GetCustomAttribute(type, typeof(Configuration)) != null);

            foreach (var type in types)
            {
                var config = Attribute.GetCustomAttribute(type, typeof(Configuration)) as Configuration;

                if (config == null)
                    continue; //this won't be null based off Where clause above, but this will make your IDE happy

                var githubConfig = configuration.GetSection(config.SectionName);
                services.Configure<type>(githubConfig);
            }
        }

        private static bool IsNotStatic(this Type t) => !t.IsAbstract && !t.IsSealed;
    }