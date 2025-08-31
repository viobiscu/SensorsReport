using SensorsReport.OrionLD;
using System.Text.Json;

namespace SensorsReport.Frontend.Common.Pages;

[Route("Dashboard/[action]")]
public class DashboardPage : Controller
{
    [PageAuthorize, HttpGet, Route("~/")]
    public ActionResult Index([FromServices] IOrionLdService orionLdService, [FromServices] ITwoLevelCache cache, [FromServices] ITenantRetriever tenantRetriever)
    {
        orionLdService.SetTenant(tenantRetriever.CurrentTenantInfo);
        var model = cache.Get<DashboardPageModel>("DashboardPageModel", TimeSpan.FromSeconds(10), "DashboardPageModel", () => {
            return new DashboardPageModel
            {
                SensorStaticsModel = SensorData(orionLdService, cache),
                AlarmStaticsModel = AlarmData(orionLdService, cache)
            };
        });

        return View(MVC.Views.Common.Dashboard.DashboardIndex, model);
    }

    public SensorStaticsModel SensorData(IOrionLdService orionLdService, ITwoLevelCache cache)
    {
        var sensorEntities = orionLdService.GetEntitiesAsync<List<EntityModel>>(0, 1000, "TG8I").ConfigureAwait(false).GetAwaiter().GetResult();
        var sensorsData = sensorEntities?.GetSensors();

        var alarmEntities = orionLdService.GetEntitiesAsync<List<EntityModel>>(0, 1000, "Alarm").ConfigureAwait(false).GetAwaiter().GetResult();
        var alarmsData = alarmEntities?.GetAlarms();

        var sensorsStatics = new SensorStaticsModel();
        if (sensorsData != null && sensorsData.Any())
        {
            sensorsStatics.SensorCount = sensorsData.Count();
            sensorsStatics.ActiveSensorCount = sensorsData.Count(s => s.Status == EntityPropertyModel.StatusValues.Operational);

            var openAlarms = alarmsData?
                .Where(s => s.Status?.Equals(AlarmModel.StatusValues.Open.Value, StringComparison.OrdinalIgnoreCase) == true);

            sensorsStatics.AlertSensorCount = openAlarms?.Select(s => $"{s.RelatedSensorGroupId}-{s.RelatedSensorName}").Distinct().Count() ?? 0;
            sensorsStatics.FaultSensors = sensorsData.Count(s => s.Status == EntityPropertyModel.StatusValues.Faulty);
        }

        return sensorsStatics;
    }

    public AlarmStaticsModel AlarmData(IOrionLdService orionLdService, ITwoLevelCache cache)
    {
        var alarmEntities = orionLdService.GetEntitiesAsync<List<EntityModel>>(0, 1000, "Alarm").ConfigureAwait(false).GetAwaiter().GetResult();
        var alarmsData = alarmEntities?.GetAlarms();
        var alarmStatics = new AlarmStaticsModel();

        if (alarmsData != null && alarmsData.Any())
        {
            alarmStatics.AlarmCount = alarmsData.Count();
            alarmStatics.PreLowAlarms = alarmsData.Count(s => s.Severity?.Equals("prelow", StringComparison.OrdinalIgnoreCase) == true && s.Status?.Equals(AlarmModel.StatusValues.Open.Value, StringComparison.OrdinalIgnoreCase) == true);
            alarmStatics.LowAlarms = alarmsData.Count(s => s.Severity?.Equals("low", StringComparison.OrdinalIgnoreCase) == true && s.Status?.Equals(AlarmModel.StatusValues.Open.Value, StringComparison.OrdinalIgnoreCase) == true);
            alarmStatics.PreHighAlarms = alarmsData.Count(s => s.Severity?.Equals("prehigh", StringComparison.OrdinalIgnoreCase) == true && s.Status?.Equals(AlarmModel.StatusValues.Open.Value, StringComparison.OrdinalIgnoreCase) == true);
            alarmStatics.HighAlarms = alarmsData.Count(s => s.Severity?.Equals("high", StringComparison.OrdinalIgnoreCase) == true && s.Status?.Equals(AlarmModel.StatusValues.Open.Value, StringComparison.OrdinalIgnoreCase) == true);
            alarmStatics.ArchivedAlarms = alarmsData.Count(s => s.Status?.Equals(AlarmModel.StatusValues.Close.Value, StringComparison.OrdinalIgnoreCase) == true);
        }
        return alarmStatics;
    }
}

public static class EntityModelExtensions
{
    public static IEnumerable<SensorDataModel> GetSensors(this IEnumerable<EntityModel> entities)
    {
        var result = new List<SensorDataModel>();
        foreach (var entity in entities)
        {
            var sensors = entity.MapSensorModel();
            result.AddRange(sensors);
        }

        return result;
    }

    public static IEnumerable<AlarmDataModel> GetAlarms(this IEnumerable<EntityModel> entities)
    {
        var result = new List<AlarmDataModel>();
        foreach (var entity in entities)
        {
            var alarm = entity.MapAlarmModel();
            result.Add(alarm);
        }
        return result;
    }

    public static AlarmDataModel MapAlarmModel(this EntityModel model)
    {
        var result = new AlarmDataModel();

        if (model.Properties == null || model.Properties?.Count <= 0)
            return result;

        result.GroupId = model.Id;
        result.GroupType = model.Type;
        result.Description = model.Properties!.FirstOrDefault(s => s.Key.Equals("description", StringComparison.OrdinalIgnoreCase)).Value.GetProperty("value").GetString();
        result.Status = model.Properties!.FirstOrDefault(s => s.Key.Equals("status", StringComparison.OrdinalIgnoreCase)).Value.GetProperty("value").GetString();
        result.Severity = model.Properties!.FirstOrDefault(s => s.Key.Equals("severity", StringComparison.OrdinalIgnoreCase)).Value.GetProperty("value").GetString();
        var monitors = model.Properties!.FirstOrDefault(s => s.Key.Equals("monitors", StringComparison.OrdinalIgnoreCase)).Value;

        string? relatedSensorGroupId = null;
        string? relatedSensorName = null;

        void ExtractMonitors(JsonElement elem)
        {
            // Unwrap 'value' if present
            if (elem.ValueKind == JsonValueKind.Object && elem.TryGetProperty("value", out var v))
                elem = v;

            // If array, take first element
            if (elem.ValueKind == JsonValueKind.Array)
                elem = elem.EnumerateArray().FirstOrDefault();

            if (elem.ValueKind == JsonValueKind.Object)
            {
                if (elem.TryGetProperty("object", out var objEl))
                {
                    if (objEl.ValueKind == JsonValueKind.Array)
                        relatedSensorGroupId = objEl.EnumerateArray().FirstOrDefault().GetString();
                    else if (objEl.ValueKind == JsonValueKind.String)
                        relatedSensorGroupId = objEl.GetString();
                }

                if (elem.TryGetProperty("monitoredAttribute", out var ma))
                {
                    if (ma.ValueKind == JsonValueKind.Object && ma.TryGetProperty("value", out var maVal))
                        relatedSensorName = maVal.GetString();
                    else if (ma.ValueKind == JsonValueKind.String)
                        relatedSensorName = ma.GetString();
                }
            }
        }

        if (monitors.ValueKind != JsonValueKind.Undefined && monitors.ValueKind != JsonValueKind.Null)
            ExtractMonitors(monitors);

        result.RelatedSensorGroupId = relatedSensorGroupId;
        result.RelatedSensorName = relatedSensorName;

        var thresholdProp = model.Properties!.FirstOrDefault(s => s.Key.Equals("threshold", StringComparison.OrdinalIgnoreCase));
        if (thresholdProp.Value.ValueKind == JsonValueKind.Object && thresholdProp.Value.TryGetProperty("value", out var thresholdValue))
        {
            result.Threshold = thresholdValue.GetDouble();
        }

        var triggeredAtProp = model.Properties!.FirstOrDefault(s => s.Key.Equals("triggeredAt", StringComparison.OrdinalIgnoreCase));
        if (triggeredAtProp.Value.ValueKind == JsonValueKind.Object && triggeredAtProp.Value.TryGetProperty("value", out var triggeredAtValue))
        {
            result.TriggeredAt = triggeredAtValue.GetDateTimeOffset();
        }

        var measuredValues = model.Properties!.FirstOrDefault(s => s.Key.Equals("measuredValues", StringComparison.OrdinalIgnoreCase)).Value;
        if (measuredValues.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in measuredValues.EnumerateArray())
            {
                var measuredItem = new AlarmDataModel.MeasuredValueItem();
                measuredItem.Value = item.GetProperty("value").GetDouble();
                measuredItem.Unit = item.GetProperty("unit").GetProperty("value").GetString();
                measuredItem.ObservedAt = item.GetProperty("observedAt").GetDateTimeOffset();
                result.MeasuredValues.Add(measuredItem);
            }
        }

        return result;
    }

    public static List<SensorDataModel> MapSensorModel(this EntityModel model)
    {
        if (model.Properties?.Count <= 0)
            return [];

        var metadataProps = model.Properties!.Select(s => s.Key).Where(s => s.StartsWith("metadata_"));
        var props = metadataProps.Select(s => s.Replace("metadata_", ""));

        var result = new List<SensorDataModel>();
        foreach (var prop in props)
        {
            var property = model.Properties!.FirstOrDefault(s => s.Key.Equals(prop, StringComparison.OrdinalIgnoreCase)).Value;
            var metadata = model.Properties!.FirstOrDefault(s => s.Key.Equals($"metadata_{prop}", StringComparison.OrdinalIgnoreCase)).Value;

            var sensor = new SensorDataModel();
            sensor.GroupId = model.Id;
            sensor.GroupType = model.Type;
            sensor.SensorName = prop;
            sensor.Name = property.GetProperty("name").GetProperty("value").GetString();
            sensor.Unit = property.GetProperty("unit").GetProperty("value").GetString();
            sensor.Value = property.GetProperty("value").GetDouble();
            sensor.ObservedAt = property.GetProperty("observedAt").GetDateTimeOffset();
            sensor.Status = metadata.GetProperty("status").GetProperty("value").GetString();
            if (metadata.TryGetProperty("Alarm", out var alarmProperty) && alarmProperty.ValueKind != JsonValueKind.Null)
                sensor.AlarmRelation = alarmProperty.GetProperty("object").GetString();

            result.Add(sensor);
        }

        return result;
    }
}

public class SensorDataModel
{
    public string? GroupId { get; set; } //eg: urn:ngsi-ld:TG8I:999998
    public string? GroupType { get; set; } //eg: TG8I
    public string? SensorName { get; set; } //eg: t0
    public string? Unit { get; set; } //eg: cel
    public string? Name { get; set; }
    public double? Value { get; set; }
    public DateTimeOffset? ObservedAt { get; set; }
    public string? Status { get; set; }
    public string? AlarmRelation { get; set; }
}

public class AlarmDataModel
{
    public string? GroupId { get; set; } //eg: urn:ngsi-ld:Alarm:3fa231a0-69aa-42b6-88b9-94221363442e
    public string? GroupType { get; set; } //eg: Alarm
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public double? Threshold { get; set; }
    public string? RelatedSensorGroupId { get; set; }
    public string? RelatedSensorName { get; set; }
    public DateTimeOffset? TriggeredAt { get; set; }
    public List<MeasuredValueItem> MeasuredValues { get; set; } = new();

    public class MeasuredValueItem
    {
        public double? Value { get; set; }
        public string? Unit { get; set; }
        public DateTimeOffset? ObservedAt { get; set; }
    }
}