using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReflectionExamples.Part1;
using ReflectionExamples.Part1.Services;
using ReflectionExamples.Part1.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Reflection.Dependencies.Part1
{
    public class CommonServiceExtensionsTests
    {
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _serviceProvider;

        public CommonServiceExtensionsTests()
        {
            _services = new ServiceCollection();
            var configRoot = AddConfigurationRoot();
            _services.AddDependencies(configRoot);
            _serviceProvider = _services.BuildServiceProvider();
        }

        [Theory]
        [InlineData(typeof(IOptions<GithubSettings>))]
        [InlineData(typeof(IOptions<GithubAuthSettings>))]
        public void Configuration_IsRegistered(Type value)
        {
            var exception = Record.Exception(() => _serviceProvider.GetRequiredService(value));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(typeof(IGithubClient), typeof(GithubClient))]
        [InlineData(typeof(IGithubAuthClient), typeof(GithubAuthClient))]
        [InlineData(typeof(IGithubService), typeof(GithubService))]
        public void TransientServices_AreRegistered(Type contract, Type concrete)
        {
            var exception = Record.Exception(() => {
                var service = _serviceProvider.GetRequiredService(contract);

                if (!contract.IsAssignableFrom(concrete))
                    throw new Exception($"Interface, {contract}, is not assignable from {concrete}");
            });
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(typeof(IGithubClient))]
        [InlineData(typeof(IGithubAuthClient))]
        [InlineData(typeof(IGithubService))]
        [InlineData(typeof(GithubDelegatingHandler))]
        public void TransientServices_AreTransient(Type contract)
        {
            var serviceLifeTime = _services.Single(x => x.ServiceType == contract).Lifetime;
            Assert.Equal(ServiceLifetime.Transient, serviceLifeTime);
        }

        [Theory]
        [InlineData(typeof(IGithubTokenCache), typeof(GithubTokenCache))]
        public void SingletonServices_AreRegistered(Type contract, Type concrete)
        {
            var exception = Record.Exception(() => {
                var service = _serviceProvider.GetRequiredService(contract);

                if (!contract.IsAssignableFrom(concrete))
                    throw new Exception($"Interface, {contract}, is not assignable from {concrete}");
            });
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(typeof(IGithubTokenCache))]
        public void SingletonServices_AreSingleton(Type contract)
        {
            //this test will fail
            var serviceLifeTime = _services.Single(x => x.ServiceType == contract).Lifetime;
            Assert.Equal(ServiceLifetime.Singleton, serviceLifeTime);
        }

        [Theory]
        [InlineData(typeof(GithubDelegatingHandler))]
        public void DelegatingHandlers_AreRegistered(Type contract)
        {
            var exception = Record.Exception(() => _serviceProvider.GetRequiredService(contract));
            Assert.Null(exception);
        }

        private IConfigurationRoot AddConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"Github:BaseUrl", "https://foo.com/"},
                {"GithubAuth:BaseUrl", "https://bar.com/"},
                {"GithubAuth:ClientId", "oweiufkldshjgiopewauhg"},
                {"GithubAuth:ClientSecret", "SuperCerealSecret"},
                {"GithubAuth:Audience", "We_The_People"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            return configuration;
        }
    }
}
