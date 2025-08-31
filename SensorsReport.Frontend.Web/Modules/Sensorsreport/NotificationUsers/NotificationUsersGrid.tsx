import { EntityGrid } from "@serenity-is/corelib";
import { NotificationUsersDialog } from "./NotificationUsersDialog";
import { NotificationUsersRow, NotificationUsersColumns, NotificationUsersService } from "../../ServerTypes/SensorsReport";

export class NotificationUsersGrid extends EntityGrid<NotificationUsersRow> {
    protected override getColumnsKey() { return NotificationUsersColumns.columnsKey; }
    protected override getDialogType() { return NotificationUsersDialog; }
    protected override getRowDefinition() { return NotificationUsersRow; }
    protected override getService() { return NotificationUsersService.baseUrl; }
}
