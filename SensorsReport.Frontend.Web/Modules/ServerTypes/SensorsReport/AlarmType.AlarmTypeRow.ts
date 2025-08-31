import { fieldsProxy } from "@serenity-is/corelib";

export interface AlarmTypeRow {
    Id?: string;
    Name?: string;
    Description?: string;
    Style?: string;
}

export abstract class AlarmTypeRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.AlarmType';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<AlarmTypeRow>();
}