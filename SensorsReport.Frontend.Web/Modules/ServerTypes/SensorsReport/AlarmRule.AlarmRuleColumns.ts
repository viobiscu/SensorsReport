import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { AlarmRuleRow } from "./AlarmRule.AlarmRuleRow";

export interface AlarmRuleColumns {
    Id: Column<AlarmRuleRow>;
    Name: Column<AlarmRuleRow>;
    Unit: Column<AlarmRuleRow>;
    Low: Column<AlarmRuleRow>;
    PreLow: Column<AlarmRuleRow>;
    PreHigh: Column<AlarmRuleRow>;
    High: Column<AlarmRuleRow>;
}

export class AlarmRuleColumns extends ColumnsBase<AlarmRuleRow> {
    static readonly columnsKey = 'SensorsReport.AlarmRule';
    static readonly Fields = fieldsProxy<AlarmRuleColumns>();
}