using System.Net.Http.Headers;

namespace SFA.DAS.TokenService.Infrastructure.Http;

public interface IHttpClientWrapper
{
    List<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; set; }

    Task<T?> Post<T>(string url, object content);
}