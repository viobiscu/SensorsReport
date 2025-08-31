import { EntityGrid } from "@serenity-is/corelib";
import { NotificationDialog } from "./NotificationDialog";
import { NotificationRow, NotificationColumns, NotificationService } from "../../ServerTypes/SensorsReport";

export class NotificationGrid extends EntityGrid<NotificationRow> {
    protected override getColumnsKey() { return NotificationColumns.columnsKey; }
    protected override getDialogType() { return NotificationDialog; }
    protected override getRowDefinition() { return NotificationRow; }
    protected override getService() { return NotificationService.baseUrl; }
}
