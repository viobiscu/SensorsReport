using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace SensorsReport;

public class HttpGlobalExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<HttpGlobalExceptionFilter> _logger;

    public HttpGlobalExceptionFilter(IHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
    {
        _env = env;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(new EventId(context.Exception.HResult),
            context.Exception,
            context.Exception.Message);

        var errorResponse = new JsonErrorResponse();
        var statusCode = (int)HttpStatusCode.InternalServerError;

        if (context.Exception is HttpRequestException httpException)
        {
            statusCode = (int)(httpException.StatusCode ?? HttpStatusCode.BadRequest);
            errorResponse.Errors = [httpException.Message];
        }
        else if (context.Exception is ValidationException validationException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            errorResponse.Errors = [validationException.Message];
        }
        else
        {
            if (_env.IsDevelopment())
            {
                errorResponse.Errors = [context.Exception.Message];
                errorResponse.DeveloperMessage = context.Exception.ToString();
            }
            else
            {
                errorResponse.Errors = ["An unexpected error occurred. Please try again later."];
            }
        }

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = statusCode
        };

        context.HttpContext.Response.StatusCode = statusCode;
        context.ExceptionHandled = true;
    }
}
