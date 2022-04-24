﻿using HttpClientExamples.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

//requires Microsoft.Extensions.Options.ConfigurationExtensions installed
namespace HttpClientExamples.Part2
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
            services.AddTransient<IGithubService, GithubService>();

            services.AddHttpClient("Github", client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);

                //github documentation recommends using this Accept header
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            });

            services.AddHttpClient("GithubAuth", client =>
            {
                client.BaseAddress = new Uri(githubClientConfig.BaseUrl);
            });
        }
    }
}
