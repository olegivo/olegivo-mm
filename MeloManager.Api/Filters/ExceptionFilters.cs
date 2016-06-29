using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace MeloManager.Api.Filters
{
    public class ProtocolExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var exception = context.Exception;
            if (exception is ProtocolViolationException)
                context.Response = context.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Format("Protocol violation: {0}", exception.Message));
        }
    }

    public class ArgumentExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var exception = context.Exception as ArgumentException;
            if (exception != null)
            {
                var error = new HttpError(exception.Message) { { "ParamName", exception.ParamName } };
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
        }
    }

    public class ValidationExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var exception = context.Exception as ValidationException;
            if (exception != null)
            {
                var error = new HttpError(exception.Message) { { "Parameters", exception.ValidationResult.MemberNames } };
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
        }
    }
}