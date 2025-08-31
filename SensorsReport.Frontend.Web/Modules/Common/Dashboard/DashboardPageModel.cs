
namespace SensorsReport.Frontend.Common;

[ScriptInclude]
public class DashboardPageModel
{
    public required SensorStaticsModel SensorStaticsModel { get; set; }
    public required AlarmStaticsModel AlarmStaticsModel { get; set; }
}


public class SensorStaticsModel
{
    public int SensorCount { get; set; }
    public int ActiveSensorCount { get; set; }
    public int AlertSensorCount { get; set; }
    public int FaultSensors { get; set; }
}

public class AlarmStaticsModel
{
    public int AlarmCount { get; set; }
    public int PreLowAlarms { get; set; }
    public int LowAlarms { get; set; }
    public int PreHighAlarms { get; set; }
    public int HighAlarms { get; set; }
    public int ArchivedAlarms { get; set; }

}
