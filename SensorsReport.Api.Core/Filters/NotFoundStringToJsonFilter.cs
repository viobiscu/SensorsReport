using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SensorsReport;

public class NotFoundStringToJsonFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is NotFoundObjectResult notFoundObjectResult && notFoundObjectResult.Value is string message)
        {
            var jsonResponse = new JsonMessageResponse { Message = message };
            context.Result = new NotFoundObjectResult(jsonResponse);
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}