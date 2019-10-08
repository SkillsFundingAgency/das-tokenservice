using System.Configuration;
using System.Web.Http;
using Microsoft.ApplicationInsights.Extensibility;
using SFA.DAS.TokenService.Infrastructure.Logging;

namespace SFA.DAS.TokenService.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];

            LoggingConfig.ConfigureLogging();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
