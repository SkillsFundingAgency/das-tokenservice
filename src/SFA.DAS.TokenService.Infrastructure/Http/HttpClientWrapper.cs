using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Http;

public class HttpClientWrapper (ILogger<HttpClientWrapper> logger) : IHttpClientWrapper
{
    public async Task<T?> Post<T>(string? url, object content)
    {
        using var client = CreateClient();

        var jsonContent = JsonConvert.SerializeObject(content);
        
        logger.LogWarning("HttpClientWrapper. POST-ing request: {Content} to Uri: {Uri}", jsonContent, url);
        
        var response = await client.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Http status code {response.StatusCode} indicates failure. (Status description: {response.ReasonPhrase}, body: {responseBody}, content: {jsonContent})");
        }

        return JsonConvert.DeserializeObject<T>(responseBody);
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.hmrc.1.0+json"));

        return client;
    }
}