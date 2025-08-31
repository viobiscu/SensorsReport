import { EntityGrid } from "@serenity-is/corelib";
import { LogRuleDialog } from "./LogRuleDialog";
import { LogRuleRow, LogRuleColumns, LogRuleService } from "../../ServerTypes/SensorsReport";

export class LogRuleGrid extends EntityGrid<LogRuleRow> {
    protected override getColumnsKey() { return LogRuleColumns.columnsKey; }
    protected override getDialogType() { return LogRuleDialog; }
    protected override getRowDefinition() { return LogRuleRow; }
    protected override getService() { return LogRuleService.baseUrl; }
}
