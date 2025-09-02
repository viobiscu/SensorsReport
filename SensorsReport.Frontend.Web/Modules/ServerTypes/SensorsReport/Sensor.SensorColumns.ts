import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { SensorRow } from "./Sensor.SensorRow";

export interface SensorColumns {
    Id: Column<SensorRow>;
    T0_Name: Column<SensorRow>;
    T0_Status: Column<SensorRow>;
    T0: Column<SensorRow>;
    T0_Unit: Column<SensorRow>;
    T0_ObservedAt: Column<SensorRow>;
    RH0_Name: Column<SensorRow>;
    RH0_Status: Column<SensorRow>;
    RH0: Column<SensorRow>;
    RH0_Unit: Column<SensorRow>;
    RH0_ObservedAt: Column<SensorRow>;
}

export class SensorColumns extends ColumnsBase<SensorRow> {
    static readonly columnsKey = 'SensorsReport.Sensor';
    static readonly Fields = fieldsProxy<SensorColumns>();
}