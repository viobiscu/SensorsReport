import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { AlarmRow } from "./Alarm.AlarmRow";

export interface AlarmColumns {
    Id: Column<AlarmRow>;
    Description: Column<AlarmRow>;
    Status: Column<AlarmRow>;
    Severity: Column<AlarmRow>;
    Monitors: Column<AlarmRow>;
    Threshold: Column<AlarmRow>;
    Condition: Column<AlarmRow>;
    MeasuredValue: Column<AlarmRow>;
}

export class AlarmColumns extends ColumnsBase<AlarmRow> {
    static readonly columnsKey = 'SensorsReport.Alarm';
    static readonly Fields = fieldsProxy<AlarmColumns>();
}