using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.TokenService.Infrastructure.DependencyResolution
{
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
}
