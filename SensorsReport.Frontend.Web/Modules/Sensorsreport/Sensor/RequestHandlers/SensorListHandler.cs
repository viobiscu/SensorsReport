using SensorsReport.Frontend.Common.Pages;
using SensorsReport.Frontend.Modules.Common.OrionLDHandlers;
using SensorsReport.OrionLD;
using System.Text.Json;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.SensorsReport.Sensor.SensorRow>;
using MyRow = SensorsReport.Frontend.SensorsReport.Sensor.SensorRow;


namespace SensorsReport.Frontend.SensorsReport.Sensor;
public interface ISensorListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class SensorListHandler(IHttpContextAccessor httpContextAccessor) :
    OrionLDListHandler<MyRow, MyRequest, MyResponse>(httpContextAccessor), ISensorListHandler
{
    protected override void ExecuteQuery()
    {
        try
        {
            orionLdService.SetTenant(GetTenantInfo());
            orionLdService.SetOptions(OrionLDOptions.Default);

            var entities = orionLdService.GetEntitiesAsync<List<EntityModel>>(0, 1000, MyRow.Fields.TableName).ConfigureAwait(false).GetAwaiter().GetResult();

            if (entities?.Any() == true)
            {
                Response.TotalCount = entities.Count;

                entities = entities.Skip(skip).Take(take).ToList();

                foreach (var entity in entities)
                {
                    var row = ResponseMapSensorModelToRow(entity, Request.IncludeColumns);
                    Response.Entities.Add(row);
                }

            }
        }
        catch (Exception exception)
        {
            foreach (var behavior in behaviors.Value.OfType<IListExceptionBehavior>())
                behavior.OnException(this, exception);

            throw;
        }
    }

    private MyRow ResponseMapSensorModelToRow(EntityModel entity, HashSet<string> includeColumns)
    {
        var row = new MyRow();
        row.Id = entity.Id;

        if (includeColumns == null)
        {
            includeColumns = [];
            foreach (var field in MyRow.Fields)
                includeColumns.Add(field.Name);
        }

        if (entity.Properties == null)
            return row;

        foreach (var column in includeColumns)
        {
            if (entity.Properties.Keys.Any(s => s.Equals(column, StringComparison.OrdinalIgnoreCase)))
            {
                var columnKey = entity.Properties.Keys.First(s => s.Equals(column, StringComparison.OrdinalIgnoreCase));
                if (entity.Properties.ContainsKey($"metadata_{columnKey}"))
                {
                    MapColumnWithMetadata(row, column, entity.Properties.GetValueOrDefault(columnKey), entity.Properties.GetValueOrDefault($"metadata_{columnKey}"));
                }
            }
        }

        return row;
    }

    private void MapColumnWithMetadata(MyRow row, string colName, JsonElement column, JsonElement metaColumn)
    {
        var sensorProp = MyRow.Fields.FindField(colName);
        var sensorPropUnit = MyRow.Fields.FindField($"{colName}_Unit");
        var sensorPropObservedAt = MyRow.Fields.FindField($"{colName}_ObservedAt");
        var sensorPropStatus = MyRow.Fields.FindField($"{colName}_Status");
        var sensorPropName = MyRow.Fields.FindField($"{colName}_Name");
        var value = column.GetProperty("value").GetDouble();
        var unit = column.GetProperty("unit").GetProperty("value").GetString();

        sensorProp.AsObject(row, value);
        sensorPropUnit.AsObject(row, unit);
        sensorPropObservedAt.AsObject(row, column.GetProperty("observedAt").GetDateTime());
        sensorPropStatus.AsObject(row, metaColumn.GetProperty("status").GetProperty("value").GetString());
        sensorPropName.AsObject(row, column.GetProperty("name").GetProperty("value").GetString());
    }
}