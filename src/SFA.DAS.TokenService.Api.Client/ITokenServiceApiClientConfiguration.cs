using System.Security.Cryptography.X509Certificates;

namespace SFA.DAS.TokenService.Api.Client
{
    public interface ITokenServiceApiClientConfiguration
    {
        string ApiBaseUrl { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string IdentifierUri { get; set; }
        string Tenant { get; set; }
        X509Certificate TokenCertificate { get; set; }
    }
}