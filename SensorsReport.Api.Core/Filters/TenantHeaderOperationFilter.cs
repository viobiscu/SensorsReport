using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        if (path.StartsWith("health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("version", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var hasTenantHeaderAttribute = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<UseTenantHeaderAttribute>()
            .Any() ?? false ||
            context.MethodInfo.GetCustomAttributes(true)
            .OfType<UseTenantHeaderAttribute>()
            .Any();

        if (!hasTenantHeaderAttribute)
            return;

        if (operation.Parameters == null)
        {
            operation.Parameters = new List<OpenApiParameter>();
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "NGSILD-Tenant",
            In = ParameterLocation.Header,
            Description = "Tenant identifier, used to scope the request to a specific tenant.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}