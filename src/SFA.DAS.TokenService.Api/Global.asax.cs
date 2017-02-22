using System.Web.Http;
using SFA.DAS.TokenService.Infrastructure.Logging;

namespace SFA.DAS.TokenService.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LoggingConfig.ConfigureLogging();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
