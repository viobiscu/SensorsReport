import { fieldsProxy } from "@serenity-is/corelib";

export interface GroupRow {
    Id?: string;
    Name?: string;
    Users?: string[];
}

export abstract class GroupRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.Group';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<GroupRow>();
}