using System.Threading.Tasks;

namespace SFA.DAS.TokenService.Api.Client
{
    internal interface ISecureHttpClient
    {
        Task<string> GetAsync(string url);
    }
}