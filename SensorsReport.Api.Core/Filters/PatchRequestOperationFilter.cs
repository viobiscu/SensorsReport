using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public class PatchRequestOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var patchAttribute = context.MethodInfo.GetCustomAttribute<PatchRequestBodyAttribute>();
            if (patchAttribute == null) return;

            var schema = new OpenApiSchema { Type = "object" };

            var patchableProperties = patchAttribute.ModelType.GetProperties()
                .Where(p => p.IsDefined(typeof(AllowPatchAttribute), false));

            foreach (var property in patchableProperties)
            {
                var propertyName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                
                var propertySchema = context.SchemaGenerator.GenerateSchema(property.PropertyType, context.SchemaRepository);
                propertySchema.Nullable = true;
                
                schema.Properties.Add(propertyName, propertySchema);
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = {
                    ["application/json"] = new OpenApiMediaType { Schema = schema }
                }
            };
        }
    }