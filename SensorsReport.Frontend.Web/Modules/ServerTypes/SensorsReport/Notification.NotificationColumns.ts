import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { NotificationRow } from "./Notification.NotificationRow";

export interface NotificationColumns {
    Id: Column<NotificationRow>;
    Name: Column<NotificationRow>;
    Enable: Column<NotificationRow>;
    NotificationRule: Column<NotificationRow>;
    NotificationUser: Column<NotificationRow>;
    SMS: Column<NotificationRow>;
    Email: Column<NotificationRow>;
    Monitors: Column<NotificationRow>;
}

export class NotificationColumns extends ColumnsBase<NotificationRow> {
    static readonly columnsKey = 'SensorsReport.Notification';
    static readonly Fields = fieldsProxy<NotificationColumns>();
}