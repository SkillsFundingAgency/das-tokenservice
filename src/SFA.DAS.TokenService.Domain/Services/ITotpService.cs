namespace SFA.DAS.TokenService.Domain.Services
{
    public interface ITotpService
    {
        string Generate(string secret);
    }
}