import { EntityGrid } from "@serenity-is/corelib";
import { UserDialog } from "./UserDialog";
import { UserRow, UserColumns, UserService } from "../../ServerTypes/SensorsReport";

export class UserGrid extends EntityGrid<UserRow> {
    protected override getColumnsKey() { return UserColumns.columnsKey; }
    protected override getDialogType() { return UserDialog; }
    protected override getRowDefinition() { return UserRow; }
    protected override getService() { return UserService.baseUrl; }
}
