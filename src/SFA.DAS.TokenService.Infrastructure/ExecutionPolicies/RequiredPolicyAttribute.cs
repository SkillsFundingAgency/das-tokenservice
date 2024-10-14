namespace SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

[AttributeUsage(AttributeTargets.Parameter)]
public class RequiredPolicyAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}