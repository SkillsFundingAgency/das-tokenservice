using System.Security.Cryptography.X509Certificates;

namespace SFA.DAS.TokenService.Api.Client
{
    public interface ITokenServiceApiClientConfiguration
    {
        string ApiBaseUrl { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string IdentifierUri { get; }
        string Tenant { get; }
        X509Certificate TokenCertificate { get; }
    }
}