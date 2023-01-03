using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace FacturadorEstacionesAPI.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;


        /// <summary>
        /// 
        /// </summary>
        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void OnException(ExceptionContext context)
        {
            ApiError apiError = null;
            if (context.Exception is DataApiException)
            {
                var exception = context.Exception as DataApiException;
                var msg = exception.Message;
                string stack = exception.StackTrace;
                apiError = new ApiError(msg) { Detail = stack };
                context.HttpContext.Response.StatusCode = 400;

                _logger.LogError(msg+ exception);
            }
            else
            {
                // Unhandled errors
                var exception = context.Exception.GetBaseException();
                var msg = exception.Message;
                string stack = context.Exception.StackTrace;

                apiError = new ApiError(msg) { Detail = stack };

                context.HttpContext.Response.StatusCode = 500;

                _logger.LogError(msg + exception);
            }

            // always return a JSON result
            context.Result = new JsonResult(apiError);

            base.OnException(context);
        }
    }
}