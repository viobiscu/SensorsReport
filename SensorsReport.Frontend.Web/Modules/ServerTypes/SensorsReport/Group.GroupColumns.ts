import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { GroupRow } from "./Group.GroupRow";

export interface GroupColumns {
    Id: Column<GroupRow>;
    Name: Column<GroupRow>;
    Users: Column<GroupRow>;
}

export class GroupColumns extends ColumnsBase<GroupRow> {
    static readonly columnsKey = 'SensorsReport.Group';
    static readonly Fields = fieldsProxy<GroupColumns>();
}