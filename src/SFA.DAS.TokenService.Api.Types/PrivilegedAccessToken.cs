namespace SFA.DAS.TokenService.Api.Types;

public class PrivilegedAccessToken
{
    public string? AccessCode { get; set; }
    public DateTime ExpiryTime { get; set; }
}