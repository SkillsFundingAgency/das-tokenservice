using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Http;

public class HttpClientWrapper : IHttpClientWrapper
{
    public async Task<T?> Post<T>(string? url, object content)
    {
        using var client = CreateClient();

        var jsonContent = JsonConvert.SerializeObject(content);
        var response = await client.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Http status code {response.StatusCode} indicates failure. (Status description: {response.ReasonPhrase}, body: {responseBody})");
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