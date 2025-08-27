import { fieldsProxy } from "@serenity-is/corelib";

export interface UserExternalLoginRow {
    UserExternalLoginId?: number;
    LoginProvider?: string;
    ProviderKey?: string;
    UserId?: number;
    Username?: string;
    UserIsActive?: number;
}

export abstract class UserExternalLoginRow {
    static readonly idProperty = 'UserExternalLoginId';
    static readonly nameProperty = 'LoginProvider';
    static readonly localTextPrefix = 'Administration.UserExternalLogin';
    static readonly deletePermission = 'Administration:General';
    static readonly insertPermission = 'Administration:General';
    static readonly readPermission = 'Administration:General';
    static readonly updatePermission = 'Administration:General';

    static readonly Fields = fieldsProxy<UserExternalLoginRow>();
}