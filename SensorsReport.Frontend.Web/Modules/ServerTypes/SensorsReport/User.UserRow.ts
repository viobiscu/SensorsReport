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
    static readonly deletePermission = 'Administration:Security';
    static readonly insertPermission = 'Administration:Security';
    static readonly readPermission = 'Administration:Security';
    static readonly updatePermission = 'Administration:Security';

    static readonly Fields = fieldsProxy<UserRow>();
}