import { fieldsProxy } from "@serenity-is/corelib";

export interface NotificationRuleRow {
    Id?: string;
    Enable?: boolean;
    Name?: string;
    ConsecutiveHits?: number;
    RepeatAfter?: number;
    NotifyIfClose?: boolean;
    NotifyIfAcknowledged?: boolean;
    RepeatIfAcknowledged?: number;
    NotifyIfTimeOut?: number;
    NotificationChannel?: string[];
}

export abstract class NotificationRuleRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.NotificationRule';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<NotificationRuleRow>();
}