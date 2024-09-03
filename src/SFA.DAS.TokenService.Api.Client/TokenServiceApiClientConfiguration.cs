namespace SFA.DAS.TokenService.Api.Client;

public class TokenServiceApiClientConfiguration : ITokenServiceApiClientConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string IdentifierUri { get; set; }
    public string Tenant { get; set; }
}