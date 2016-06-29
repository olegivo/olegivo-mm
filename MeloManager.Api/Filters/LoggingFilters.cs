using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MeloManager.Api.Filters
{
    public class LogActionFilter : ActionFilterAttribute
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (log.IsTraceEnabled)
            {
                var actionContext = actionExecutedContext.ActionContext;
                if (actionExecutedContext.Exception != null)
                {
                    log.TraceException(
                        string.Format("{0} executed, resulted exception", GetFullMethod(actionContext)),
                        actionExecutedContext.Exception);
                }
                else if (actionExecutedContext.Response.Content is ObjectContent<HttpError>)
                {
                    var error = (HttpError)(actionExecutedContext.Response.Content as ObjectContent<HttpError>).Value;
                    log.Trace("{0} executed, status {1}, error message: {2}",
                        GetFullMethod(actionContext),
                        actionContext.Response.StatusCode,
                        error.Message);
                }
                else
                {
                    log.Trace("{0} executed, status {1}", GetFullMethod(actionContext), actionContext.Response.StatusCode);
                }
            }
            base.OnActionExecuted(actionExecutedContext);
        }

        private static string GetFullMethod(HttpActionContext actionContext)
        {
            return string.Format("{0} {1}/{2}",
                actionContext.Request.Method,
                actionContext.ControllerContext.ControllerDescriptor.ControllerName,
                actionContext.ActionDescriptor.ActionName);
        }
    }

    public class UnknownExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null && actionExecutedContext.Response == null)
            {
                //var log = NLog.LogManager.GetClassLogger(actionExecutedContext.ActionContext.ControllerContext.Controller.GetType());
                var log = NLog.LogManager.GetLogger(actionExecutedContext.ActionContext.ControllerContext.Controller.GetType().FullName);
                log.ErrorException("Unhandled action exception", actionExecutedContext.Exception);
            }
            base.OnException(actionExecutedContext);
        }
    }
}