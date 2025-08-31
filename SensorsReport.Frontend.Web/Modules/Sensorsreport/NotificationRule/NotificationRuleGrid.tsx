import { EntityGrid } from "@serenity-is/corelib";
import { NotificationRuleDialog } from "./NotificationRuleDialog";
import { NotificationRuleRow, NotificationRuleColumns, NotificationRuleService } from "../../ServerTypes/SensorsReport";

export class NotificationRuleGrid extends EntityGrid<NotificationRuleRow> {
    protected override getColumnsKey() { return NotificationRuleColumns.columnsKey; }
    protected override getDialogType() { return NotificationRuleDialog; }
    protected override getRowDefinition() { return NotificationRuleRow; }
    protected override getService() { return NotificationRuleService.baseUrl; }
}
