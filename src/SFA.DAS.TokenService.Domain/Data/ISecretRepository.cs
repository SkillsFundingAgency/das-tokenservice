namespace SFA.DAS.TokenService.Domain.Data;

public interface ISecretRepository
{
    Task<string> GetSecretAsync(string name);
}