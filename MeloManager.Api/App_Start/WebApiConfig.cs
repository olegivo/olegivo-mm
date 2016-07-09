using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Web.Http;
using MeloManager.Api.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MeloManager.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Formatting.Indented;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.Converters.Add(new StringEnumConverter());

            //filters
            config.Filters.Add(new UnknownExceptionFilter());
            config.Filters.Add(new LogActionFilter());
            config.Filters.Add(new ValidationExceptionFilter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            RegisterApiRoutes(config.Routes);
        }

        private static void RegisterApiRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            /*routes.MapHttpRoute(
                name: "ApiControllerAction",
                routeTemplate: "{controller}/{action}",
                defaults: new { action = "default" },
                constraints: new { controller = "files" }
            );*/

            routes.MapHttpRoute(
               name: "ApiController",
               routeTemplate: "{controller}",
               defaults: new { controller = "Default" }
            );
            routes.MapHttpRoute(
               name: "ApiControllerIntegerId",
               routeTemplate: "{controller}/{id}",
               defaults: new { },
               constraints: new { id = @"\d+" }
            );
        }
    }
}
