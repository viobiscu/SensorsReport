
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public class ExcludeTenantSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null)
        {
            return;
        }

        // 'tenant' özelliğini büyük/küçük harfe duyarsız olarak bul
        var tenantProperty = schema.Properties.Keys
            .FirstOrDefault(key => key.Equals("tenant", StringComparison.OrdinalIgnoreCase));

        if (tenantProperty != null)
        {
            schema.Properties.Remove(tenantProperty);
        }
    }
}
