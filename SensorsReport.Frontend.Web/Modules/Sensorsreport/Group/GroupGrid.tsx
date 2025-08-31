import { EntityGrid } from "@serenity-is/corelib";
import { GroupDialog } from "./GroupDialog";
import { GroupRow, GroupColumns, GroupService } from "../../ServerTypes/SensorsReport";

export class GroupGrid extends EntityGrid<GroupRow> {
    protected override getColumnsKey() { return GroupColumns.columnsKey; }
    protected override getDialogType() { return GroupDialog; }
    protected override getRowDefinition() { return GroupRow; }
    protected override getService() { return GroupService.baseUrl; }
}
