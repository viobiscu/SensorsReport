using System.Text.Json.Serialization;

namespace SensorsReport.Frontend.Common;

public abstract class OLDRow<TFields> : Row<TFields> where TFields : RowFieldsBase
{
    [JsonPropertyName("type")]
    public string Type { get => this.GetType().GetAttribute<TableNameAttribute>()!.Name; }
}