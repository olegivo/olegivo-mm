using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Oleg_ivo.Base.Autofac.DependencyInjection;

namespace MeloManager.Api
{
    public class ContainerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.RegisterModule<PropertyInjectionModule>();
            builder.RegisterModule<ConfigurationActionsModule>();

            // Register the Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterWebApiFilterProvider(config);

            builder.Register(c => new HttpContextWrapper(HttpContext.Current)).As<HttpContextBase>().InstancePerApiRequest();
            builder.RegisterModule(new SqlDbConnectionModule(ConfigurationManager.ConnectionStrings["MeloManager"].ConnectionString));
            //builder.Register(c => new NewbuildDataContext(c.Resolve<DbConnection>()));

            // Build the container.
            var container = builder.Build();

            // Configure Web API with the dependency resolver.
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}