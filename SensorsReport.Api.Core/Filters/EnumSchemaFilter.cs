using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensorsReport;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        var sb = new StringBuilder(schema.Description);
        sb.AppendLine("<p><b>Members:</b></p><ul>");

        foreach (var enumName in Enum.GetNames(context.Type))
        {
            var member = context.Type.GetMember(enumName).FirstOrDefault();
            if (member == null) continue;

            var enumValue = Convert.ChangeType(Enum.Parse(context.Type, enumName), Enum.GetUnderlyingType(context.Type));
            var descriptionAttribute = member.GetCustomAttribute<DescriptionAttribute>();

            sb.Append($"<li><b>{enumValue} ({enumName})</b>");

            if (descriptionAttribute != null)
            {
                sb.Append($": {descriptionAttribute.Description}");
            }
            sb.AppendLine("</li>");
        }
        sb.AppendLine("</ul>");
        schema.Description = sb.ToString();
    }
}