import { fieldsProxy } from "@serenity-is/corelib";

export interface SensorRow {
    Id?: string;
    T0_Name?: string;
    T0?: number;
    T0_Unit?: string;
    T0_ObservedAt?: string;
    T0_Status?: string;
    RH0_Name?: string;
    RH0?: number;
    RH0_Unit?: string;
    RH0_ObservedAt?: string;
    RH0_Status?: string;
}

export abstract class SensorRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Id';
    static readonly localTextPrefix = 'SensorsReport.Sensor';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<SensorRow>();
}