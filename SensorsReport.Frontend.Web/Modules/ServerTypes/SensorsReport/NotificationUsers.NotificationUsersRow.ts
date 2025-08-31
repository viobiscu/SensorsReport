import { fieldsProxy } from "@serenity-is/corelib";
import { RelationModel } from "../RelationModel";

export interface NotificationUsersRow {
    Id?: string;
    Enable?: boolean;
    Name?: string;
    Notification?: string[];
    Users?: RelationModel<string>[];
    Groups?: RelationModel<string>[];
}

export abstract class NotificationUsersRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.NotificationUsers';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<NotificationUsersRow>();
}