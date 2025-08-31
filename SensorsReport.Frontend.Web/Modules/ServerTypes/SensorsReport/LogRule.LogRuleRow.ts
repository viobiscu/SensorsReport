import { fieldsProxy } from "@serenity-is/corelib";

export interface LogRuleRow {
    Id?: string;
    Name?: string;
    Unit?: string;
    Low?: number;
    High?: number;
    ConsecutiveHit?: number;
    Enabled?: boolean;
}

export abstract class LogRuleRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.LogRule';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<LogRuleRow>();
}