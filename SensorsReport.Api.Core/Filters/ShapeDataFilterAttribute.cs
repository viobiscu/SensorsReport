using System;
using System.Dynamic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SensorsReport;
public class ShapeDataFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Query.TryGetValue("fields", out var fields))
        {
            base.OnActionExecuting(context);
            return;
        }
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult || objectResult.Value == null)
        {
            base.OnResultExecuting(context);
            return;
        }

        if (!context.HttpContext.Request.Query.TryGetValue("fields", out var fields) || string.IsNullOrWhiteSpace(fields))
        {
            base.OnResultExecuting(context);
            return;
        }

        var fieldList = fields.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (objectResult.Value is IEnumerable<object> enumerable)
        {
            objectResult.Value = enumerable.Select(item => ShapeObject(item, fieldList));
        }
        else
        {
            objectResult.Value = ShapeObject(objectResult.Value, fieldList);
        }

        base.OnResultExecuting(context);
    }

    private static ExpandoObject ShapeObject(object source, IEnumerable<string> fields)
    {
        var shapedObject = new ExpandoObject();
        var shapedObjectDictionary = (IDictionary<string, object?>)shapedObject;
        var sourceType = source.GetType();

        foreach (var field in fields)
        {
            var property = sourceType.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property != null && property.GetCustomAttribute<ResponseIgnoreAttribute>() == null)
            {
                shapedObjectDictionary.Add(property.Name, property.GetValue(source));
            }
        }

        return shapedObject;
    }
}