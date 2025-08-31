import { fieldsProxy } from "@serenity-is/corelib";

export interface AlarmRuleRow {
    Id?: string;
    Name?: string;
    Unit?: string;
    Low?: number;
    PreLow?: number;
    PreHigh?: number;
    High?: number;
}

export abstract class AlarmRuleRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.AlarmRule';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<AlarmRuleRow>();
}