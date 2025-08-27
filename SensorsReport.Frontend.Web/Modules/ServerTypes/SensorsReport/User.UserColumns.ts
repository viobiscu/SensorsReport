import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { UserRow } from "./User.UserRow";

export interface UserColumns {
    Id: Column<UserRow>;
    Username: Column<UserRow>;
    Email: Column<UserRow>;
    FirstName: Column<UserRow>;
    LastName: Column<UserRow>;
    Mobile: Column<UserRow>;
    Language: Column<UserRow>;
}

export class UserColumns extends ColumnsBase<UserRow> {
    static readonly columnsKey = 'SensorsReport.User';
    static readonly Fields = fieldsProxy<UserColumns>();
}