import { getLookup, getLookupAsync, fieldsProxy } from "@serenity-is/corelib";

export interface SensorHistoryRow {
    Id?: number;
    Tenant?: string;
    SensorId?: string;
    PropertyKey?: string;
    MetadataKey?: string;
    ObservedAt?: string;
    Value?: number;
    Unit?: string;
    CreatedAt?: string;
}

export abstract class SensorHistoryRow {
    static readonly idProperty = 'Id';
    static readonly localTextPrefix = 'SensorsReport.SensorHistory';
    static readonly lookupKey = 'SensorsReport.SensorHistory';

    /** @deprecated use getLookupAsync instead */
    static getLookup() { return getLookup<SensorHistoryRow>('SensorsReport.SensorHistory') }
    static async getLookupAsync() { return getLookupAsync<SensorHistoryRow>('SensorsReport.SensorHistory') }

    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<SensorHistoryRow>();
}