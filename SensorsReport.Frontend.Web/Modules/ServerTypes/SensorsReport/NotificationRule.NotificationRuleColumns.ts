import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { NotificationRuleRow } from "./NotificationRule.NotificationRuleRow";

export interface NotificationRuleColumns {
    Id: Column<NotificationRuleRow>;
    Name: Column<NotificationRuleRow>;
    Enable: Column<NotificationRuleRow>;
    ConsecutiveHits: Column<NotificationRuleRow>;
    RepeatAfter: Column<NotificationRuleRow>;
    NotifyIfClose: Column<NotificationRuleRow>;
    NotifyIfAcknowledged: Column<NotificationRuleRow>;
    RepeatIfAcknowledged: Column<NotificationRuleRow>;
    NotifyIfTimeOut: Column<NotificationRuleRow>;
    NotificationChannel: Column<NotificationRuleRow>;
}

export class NotificationRuleColumns extends ColumnsBase<NotificationRuleRow> {
    static readonly columnsKey = 'SensorsReport.NotificationRule';
    static readonly Fields = fieldsProxy<NotificationRuleColumns>();
}