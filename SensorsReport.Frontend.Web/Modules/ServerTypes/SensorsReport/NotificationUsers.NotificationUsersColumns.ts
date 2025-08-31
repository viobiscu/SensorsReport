import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { NotificationUsersRow } from "./NotificationUsers.NotificationUsersRow";

export interface NotificationUsersColumns {
    Id: Column<NotificationUsersRow>;
    Name: Column<NotificationUsersRow>;
    Enable: Column<NotificationUsersRow>;
}

export class NotificationUsersColumns extends ColumnsBase<NotificationUsersRow> {
    static readonly columnsKey = 'SensorsReport.NotificationUsers';
    static readonly Fields = fieldsProxy<NotificationUsersColumns>();
}