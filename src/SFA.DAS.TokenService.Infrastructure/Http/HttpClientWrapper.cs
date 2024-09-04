using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Http;

public class HttpClientWrapper : IHttpClientWrapper
{
    public List<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; set; } = [];

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

    private HttpClient CreateClient()
    {
        var client = new HttpClient();

        if (AcceptHeaders.Count <= 0)
        {
            return client;
        }
        
        client.DefaultRequestHeaders.Accept.Clear();
        
        foreach (var accept in AcceptHeaders)
        {
            client.DefaultRequestHeaders.Accept.Add(accept);
        }

        return client;
    }
}