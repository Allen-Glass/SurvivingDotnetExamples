namespace ReflectionExamples.Part4.Attributes
{
    public class Client : Attribute
    {
        public Client(Type httpClientFactoryInterface, Type httpClientFactoryConcrete, Type settings)
        {
            HttpClientFactoryInterface = httpClientFactoryInterface;
            HttpClientFactoryConcrete = httpClientFactoryConcrete;

            SetBaseUrl(settings);
        }

        public Client(Type httpClientFactoryInterface, Type httpClientFactoryConcrete, Type settings, Type delegatingHandler)
        {
            HttpClientFactoryInterface = httpClientFactoryInterface;
            HttpClientFactoryConcrete = httpClientFactoryConcrete;
            DelegatingHandler = delegatingHandler;

            SetBaseUrl(settings);
        }

        public Type HttpClientFactoryInterface { get; set; }

        public Type HttpClientFactoryConcrete { get; set; }

        public Type Settings { get; set; }

        public Type DelegatingHandler { get; set; }

        public string BaseUrl { get; set; }

        private void SetBaseUrl(Type settings)
        {
            if (!settings.GetProperties().Any(x => x.Name == "BaseUrl"))
                return;

            BaseUrl = settings.GetProperties().Single(x => x.Name == "BaseUrl").GetValue(this) as string;
        }
    }
}
