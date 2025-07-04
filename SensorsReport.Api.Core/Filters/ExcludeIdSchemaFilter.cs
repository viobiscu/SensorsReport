
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public class ExcludeIdSchemaFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody == null)
            return;

        var bodyParameter = context.ApiDescription.ParameterDescriptions
            .FirstOrDefault(p => p.Source == BindingSource.Body);

        if (bodyParameter == null)
            return;

        var propertiesToExclude = bodyParameter.ModelMetadata.ModelType.GetProperties()
            .Where(p => p.GetCustomAttribute<ExcludeFromRequestAttribute>() != null)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!propertiesToExclude.Any())
            return;

        foreach (var content in operation.RequestBody.Content.Values)
        {
            if (content.Schema?.Reference == null)
                continue;

            var originalSchemaId = content.Schema.Reference.Id;
            if (!context.SchemaRepository.Schemas.TryGetValue(originalSchemaId, out var originalSchema))
                continue;

            if (!originalSchema.Properties.Keys.Any(k => propertiesToExclude.Contains(k)))
                continue;

            var requestSchemaId = $"{originalSchemaId}Request";

            if (!context.SchemaRepository.Schemas.ContainsKey(requestSchemaId))
            {
                var requestSchema = new OpenApiSchema
                {
                    Type = originalSchema.Type,
                    Properties = new Dictionary<string, OpenApiSchema>(
                        originalSchema.Properties.Where(p => !propertiesToExclude.Contains(p.Key)),
                        StringComparer.OrdinalIgnoreCase
                    ),
                    Required = new HashSet<string>(
                        originalSchema.Required.Where(r => !propertiesToExclude.Contains(r))
                    )
                };
                context.SchemaRepository.AddDefinition(requestSchemaId, requestSchema);
            }

            content.Schema.Reference = new OpenApiReference { Id = requestSchemaId, Type = ReferenceType.Schema };
        }
    }
}