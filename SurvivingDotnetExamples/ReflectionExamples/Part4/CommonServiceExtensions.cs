using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReflectionExamples.Part4.Attributes;
using ReflectionExamples.Part4.Services;
using ReflectionExamples.Part4.Services.Models;
using System.Net.Http.Headers;
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
            services.AddHttpClientConfiguration(assembly);
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
                if (Attribute.GetCustomAttribute(type, typeof(Configuration)) is not Configuration config)
                    continue; //this won't be null based off Where clause above, but this will make your IDE happy

                config.SetSectionName(nameof(type));
                var obj = Activator.CreateInstance(type);

                if (obj == null)
                    continue;

                configuration.GetSection(config.SectionName).Bind(obj);
                services.AddSingleton(obj);
            }
        }

        private static void AddHttpClientConfiguration(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(type => Attribute.GetCustomAttribute(type, typeof(Client)) != null);

            foreach (var type in types)
            {
                if (Attribute.GetCustomAttribute(type, typeof(Client)) is not Client attribute)
                    continue;

                //The method we want
                //public static IHttpClientBuilder AddHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, Action<HttpClient> configureClient)
                var method = services.GetType()
                    .GetMethods()
                    .Where(m => m.Name == "AddHttpClient")
                    .Select(m => new {
                        Method = m,
                        Params = m.GetParameters(),
                        GenericArgs = m.GetGenericArguments()
                    })
                    .Where(x => x.Params.Length == 2
                                && x.GenericArgs.Length == 2
                                && x.Params[1].GetType() == typeof(Action<HttpClient>))
                    .Select(x => x.Method)
                    .First();

                var configuredMethod = method.MakeGenericMethod(attribute.HttpClientFactoryInterface, attribute.HttpClientFactoryConcrete);
                Action<HttpClient> configureClient = client =>
                {
                    client.BaseAddress = new Uri(attribute.BaseUrl);
                };

                //first param is null as "this" is a static class
                var methodParams = new object[] { services, configureClient };
                var httpClientBuilder = configuredMethod.Invoke(null, methodParams) as IHttpClientBuilder;

                if (attribute.DelegatingHandler == null)
                {
                    //take the one AddHttpMessageHandler that has the generic argument
                    //public static IHttpClientBuilder AddHttpMessageHandler<THandler>(this IHttpClientBuilder builder) 
                    var delegatingHandlerMethod = services.GetType()
                        .GetMethods()
                        .Where(m => m.Name == "AddHttpMessageHandler")
                        .Select(m => new {
                            Method = m,
                            GenericArgs = m.GetGenericArguments()
                        })
                        .Where(x => x.GenericArgs.Length == 2)
                        .Select(x => x.Method)
                        .First();

                    var configuredDelegatingHandlerMethod = delegatingHandlerMethod.MakeGenericMethod(attribute.DelegatingHandler);
                    configuredDelegatingHandlerMethod.Invoke(null, new object[] { httpClientBuilder });
                }
            }
        }

        private static bool IsNotStatic(this Type t) => !t.IsAbstract && !t.IsSealed;
    }
}
