import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { SensorHistoryRow } from "./SensorHistory.SensorHistoryRow";

export interface SensorHistoryColumns {
    Id: Column<SensorHistoryRow>;
    Tenant: Column<SensorHistoryRow>;
    SensorId: Column<SensorHistoryRow>;
    PropertyKey: Column<SensorHistoryRow>;
    MetadataKey: Column<SensorHistoryRow>;
    ObservedAt: Column<SensorHistoryRow>;
    Value: Column<SensorHistoryRow>;
    Unit: Column<SensorHistoryRow>;
    CreatedAt: Column<SensorHistoryRow>;
}

export class SensorHistoryColumns extends ColumnsBase<SensorHistoryRow> {
    static readonly columnsKey = 'SensorsReport.SensorHistory';
    static readonly Fields = fieldsProxy<SensorHistoryColumns>();
}