import { fieldsProxy } from "@serenity-is/corelib";
import { MeasuredModel } from "../MeasuredModel";
import { RelationModel } from "../RelationModel";

export interface AlarmRow {
    Id?: string;
    Description?: string;
    Status?: string;
    Severity?: string;
    Monitors?: RelationModel<string>[];
    Threshold?: number;
    Condition?: string;
    MeasuredValue?: MeasuredModel<number>[];
}

export abstract class AlarmRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Description';
    static readonly localTextPrefix = 'SensorsReport.Alarm';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<AlarmRow>();
}