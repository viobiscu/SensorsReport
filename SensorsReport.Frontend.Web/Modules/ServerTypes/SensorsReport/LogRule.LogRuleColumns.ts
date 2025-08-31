import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { LogRuleRow } from "./LogRule.LogRuleRow";

export interface LogRuleColumns {
    Id: Column<LogRuleRow>;
    Name: Column<LogRuleRow>;
    Unit: Column<LogRuleRow>;
    Low: Column<LogRuleRow>;
    High: Column<LogRuleRow>;
    ConsecutiveHit: Column<LogRuleRow>;
    Enabled: Column<LogRuleRow>;
}

export class LogRuleColumns extends ColumnsBase<LogRuleRow> {
    static readonly columnsKey = 'SensorsReport.LogRule';
    static readonly Fields = fieldsProxy<LogRuleColumns>();
}