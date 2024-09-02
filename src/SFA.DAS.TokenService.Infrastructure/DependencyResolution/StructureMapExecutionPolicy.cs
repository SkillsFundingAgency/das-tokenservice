using System.Reflection;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Infrastructure.DependencyResolution;

public class StructureMapExecutionPolicy : ConfiguredInstancePolicy
{
    protected override void apply(Type pluginType, IConfiguredInstance instance)
    {
        var policies = instance?.Constructor?.GetParameters().Where(x => x.ParameterType == typeof(ExecutionPolicy)) ?? new ParameterInfo[0];
        foreach (var policyDependency in policies)
        {
            var policyName = policyDependency.GetCustomAttribute<RequiredPolicyAttribute>()?.Name;
            instance.Dependencies.AddForConstructorParameter(policyDependency, new ReferencedInstance(policyName));
        }
    }
}