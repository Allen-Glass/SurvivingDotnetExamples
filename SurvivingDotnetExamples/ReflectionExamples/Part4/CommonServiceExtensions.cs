using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part4.Attributes;
using System.Reflection;

namespace ReflectionExamples.Part4
{
    public static class CommonServiceExtensions
    {
        public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(CommonServiceExtensions).Assembly;

            services.AddServices(assembly);
            services.AddConfiguration(configuration, assembly);
            services.AddHttpClientConfiguration(configuration, assembly);
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
                var configurationSection = configuration.GetConfigurationSection(type);

                var method = typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethods()
                    .Where(m => m.Name == "Configure")
                    .Select(m => new
                    {
                        Method = m,
                        Params = m.GetParameters(),
                    })
                    .Where(x => x.Params.Length == 2)
                    .Select(x => x.Method)
                    .First()
                    .MakeGenericMethod(type);

                method.Invoke(null, new object[] { services, configurationSection });
            }
        }

        private static void AddHttpClientConfiguration(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(type => Attribute.GetCustomAttribute(type, typeof(Client)) != null);

            foreach (var type in types)
            {
                if (Attribute.GetCustomAttribute(type, typeof(Client)) is not Client attribute)
                    continue;

                var attributeType = attribute.GetType();
                var attributeProperties = attributeType.GetProperties();

                var namespacePath = typeof(CommonServiceExtensions).Namespace;
                var truncatedName = type.Name.Replace("Client", "");

                var httpClientFactoryInterfaceType = assembly.GetType($"{namespacePath}.Services.I{truncatedName}HttpClientFactory");
                var httpClientFactoryConcreteType = assembly.GetType($"{namespacePath}.Services.{truncatedName}HttpClientFactory");
                var delegatingHandlerType = assembly.GetType($"{namespacePath}.Services.{truncatedName}DelegatingHandler");
                var settingsType = assembly.GetType($"{namespacePath}.Services.Models.{truncatedName}Settings");

                var configurationSection = configuration.GetConfigurationSection(settingsType);
                var useableConfiguration = configurationSection.Get(settingsType);
                var baseUrl = useableConfiguration.GetType().GetProperties().Single(x => x.Name == "BaseUrl").GetValue(useableConfiguration) as string;


                //The method we want
                //public static IHttpClientBuilder AddHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, Action<HttpClient> configureClient)
                var method = typeof(HttpClientFactoryServiceCollectionExtensions)
                    .GetMethods()
                    .Where(m => m.Name == "AddHttpClient")
                    .Select(m => new
                    {
                        Method = m,
                        Params = m.GetParameters(),
                        GenericArgs = m.GetGenericArguments()
                    })
                    .Where(x => x.Params.Length == 2 && x.GenericArgs.Length == 2)
                    .Select(x => x.Method)
                    .First(x => x.GetParameters()[1].ParameterType == typeof(Action<HttpClient>))
                    .MakeGenericMethod(httpClientFactoryInterfaceType, httpClientFactoryConcreteType);

                Action<HttpClient> configureClient = client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                };

                var httpClientBuilder = method.Invoke(null, new object[] { services, configureClient }) as IHttpClientBuilder;

                if (delegatingHandlerType != null)
                    services.RegisterDelegatingHandler(httpClientBuilder, delegatingHandlerType);
            }
        }

        private static void RegisterDelegatingHandler(this IServiceCollection services, IHttpClientBuilder httpClientBuilder, Type delegatingHandler)
        {
            //take the one AddHttpMessageHandler that has the generic argument
            //public static IHttpClientBuilder AddHttpMessageHandler<THandler>(this IHttpClientBuilder builder) 
            var delegatingHandlerMethod = typeof(HttpClientBuilderExtensions)
                        .GetMethods()
                        .Where(m => m.Name == "AddHttpMessageHandler")
                        .Select(m => new {
                            Method = m,
                            GenericArgs = m.GetGenericArguments()
                        })
                        .Where(x => x.GenericArgs.Length == 1)
                        .Select(x => x.Method)
                        .First();

            var configuredDelegatingHandlerMethod = delegatingHandlerMethod.MakeGenericMethod(delegatingHandler);
            configuredDelegatingHandlerMethod.Invoke(null, new object[] { httpClientBuilder });
            services.AddTransient(delegatingHandler);
        }

        private static IConfigurationSection GetConfigurationSection(this IConfiguration configuration, Type type)
        {
            if (Attribute.GetCustomAttribute(type, typeof(Configuration)) is not Configuration config)
                throw new ArgumentException($"Type, {type.FullName}, does not have Configuration attribute");

            config.SetSectionName(type.Name);

            return configuration.GetSection(config.SectionName);
        }

        private static bool IsNotStatic(this Type t) => !t.IsAbstract && !t.IsSealed;
    }
}
