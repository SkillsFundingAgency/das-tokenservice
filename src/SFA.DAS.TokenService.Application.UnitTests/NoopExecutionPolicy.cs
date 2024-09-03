using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.UnitTests;

public class NoopExecutionPolicy : ExecutionPolicy
{
    public override async Task<T?> ExecuteAsync<T>(Func<Task<T>> func) where T : default
    {
        return await func();
    }
}