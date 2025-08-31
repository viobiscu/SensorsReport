import { EntityGrid } from "@serenity-is/corelib";
import { AlarmRuleDialog } from "./AlarmRuleDialog";
import { AlarmRuleRow, AlarmRuleColumns, AlarmRuleService } from "../../ServerTypes/SensorsReport";

export class AlarmRuleGrid extends EntityGrid<AlarmRuleRow> {
    protected override getColumnsKey() { return AlarmRuleColumns.columnsKey; }
    protected override getDialogType() { return AlarmRuleDialog; }
    protected override getRowDefinition() { return AlarmRuleRow; }
    protected override getService() { return AlarmRuleService.baseUrl; }
}
