import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { AlarmTypeRow } from "./AlarmType.AlarmTypeRow";

export interface AlarmTypeColumns {
    Id: Column<AlarmTypeRow>;
    Name: Column<AlarmTypeRow>;
    Description: Column<AlarmTypeRow>;
    Style: Column<AlarmTypeRow>;
}

export class AlarmTypeColumns extends ColumnsBase<AlarmTypeRow> {
    static readonly columnsKey = 'SensorsReport.AlarmType';
    static readonly Fields = fieldsProxy<AlarmTypeColumns>();
}