// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using MediatR;
using Microsoft.Azure;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using StructureMap;
using StructureMap.Graph;

namespace SFA.DAS.TokenService.Api.DependencyResolution
{

    public class DefaultRegistry : Registry
    {
        private const string ServiceNamespace = "SFA.DAS";

        public DefaultRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceNamespace));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
            });

            
            RegisterMediator();
            RegisterExecutionPolicies();
            RegisterConfiguration();
        }

        private void RegisterMediator()
        {
            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            For<IMediator>().Use<Mediator>();
        }
        private void RegisterConfiguration()
        {
            For<KeyVaultConfiguration>().Use(() => new KeyVaultConfiguration
            {
                VaultUri = CloudConfigurationManager.GetSetting("KeyVaultUri"),
                ClientId = CloudConfigurationManager.GetSetting("KeyVaultClientId"),
                ClientSecret = CloudConfigurationManager.GetSetting("KeyVaultClientSecret")
            });
            For<OAuthTokenServiceConfiguration>().Use(() => new OAuthTokenServiceConfiguration
            {
                Url = CloudConfigurationManager.GetSetting("HmrcTokenUri"),
                ClientId = CloudConfigurationManager.GetSetting("HmrcTokenClientId")
            });
        }

        private void RegisterExecutionPolicies()
        {
            For<Infrastructure.ExecutionPolicies.ExecutionPolicy>()
                .Use<Infrastructure.ExecutionPolicies.HmrcExecutionPolicy>()
                .Named(Infrastructure.ExecutionPolicies.HmrcExecutionPolicy.Name);
        }
    }
}