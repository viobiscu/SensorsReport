import { fieldsProxy } from "@serenity-is/corelib";
import { RelationModel } from "../RelationModel";

export interface NotificationRow {
    Id?: string;
    Enable?: boolean;
    Name?: string;
    NotificationRule?: string;
    NotificationUser?: string;
    SMS?: RelationModel<string>[];
    Email?: RelationModel<string>[];
    Monitors?: RelationModel<string>[];
}

export abstract class NotificationRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.Notification';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<NotificationRow>();
}