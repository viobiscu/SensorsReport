import { fieldsProxy } from "@serenity-is/corelib";

export interface ApiKeyRow {
    Id?: string;
    TenantId?: string;
    ApiKey?: string;
}

export abstract class ApiKeyRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'TenantId';
    static readonly localTextPrefix = 'SensorsReport.ApiKey';
    static readonly deletePermission = 'Administration:Security';
    static readonly insertPermission = 'Administration:Security';
    static readonly readPermission = 'Administration:Security';
    static readonly updatePermission = 'Administration:Security';

    static readonly Fields = fieldsProxy<ApiKeyRow>();
}