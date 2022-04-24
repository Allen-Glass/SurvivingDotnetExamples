namespace ReflectionExamples.Part0.Services;

public static class HttpClientExtensions
{
    public static async Task<string> GetJsonAsync(this HttpClient httpClient, string url)
    {
        using var httpResponseMessage = await httpClient.GetAsync(url);
        var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

        httpResponseMessage.EnsureSuccessfulRequest(responseStringContent);

        return responseStringContent;
    }

    public static async Task<string> PostJsonAsync(this HttpClient httpClient, string url, string json)
    {
        var httpContent = new StringContent(json);
        using var httpResponseMessage = await httpClient.PostAsync(url, httpContent);
        var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

        httpResponseMessage.EnsureSuccessfulRequest(responseStringContent);

        return responseStringContent;
    }

    public static async Task<string> PostUrlFormAsync(this HttpClient httpClient, string url, Dictionary<string, string> keyValuePairs)
    {
        var urlEncodedContent = new FormUrlEncodedContent(keyValuePairs);

        using var httpResponseMessage = await httpClient.PostAsync(url, urlEncodedContent);
        var responseStringContent = await httpResponseMessage.Content.ReadAsStringAsync();

        httpResponseMessage.EnsureSuccessfulRequest(responseStringContent);

        return responseStringContent;
    }

    private static void EnsureSuccessfulRequest(this HttpResponseMessage httpResponseMessage, string responseStringContent)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
            throw new HttpRequestException($"There was an issue sending a request. Status Code: {httpResponseMessage.StatusCode}" +
                $" See exception details: {responseStringContent}");
    }
}
