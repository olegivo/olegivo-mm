using System.Web.Http;
using System.Web.Routing;
using NLog;

namespace MeloManager.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            log.Debug("Preparing configuration");
            GlobalConfiguration.Configure(config =>
            {
                ContainerConfig.Register(config);
                WebApiConfig.Register(config);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                config.EnsureInitialized();
            });

            log.Info("Service started");
        }
    }
}
