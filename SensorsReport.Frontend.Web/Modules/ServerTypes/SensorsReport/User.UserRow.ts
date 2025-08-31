import { fieldsProxy } from "@serenity-is/corelib";

export interface UserRow {
    Id?: string;
    Username?: string;
    Email?: string;
    FirstName?: string;
    LastName?: string;
    Mobile?: string;
    Language?: string;
}

export abstract class UserRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Username';
    static readonly localTextPrefix = 'SensorsReport.User';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<UserRow>();
}