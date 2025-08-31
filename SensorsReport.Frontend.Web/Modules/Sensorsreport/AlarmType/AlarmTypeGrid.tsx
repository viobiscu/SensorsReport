import { EntityGrid } from "@serenity-is/corelib";
import { AlarmTypeDialog } from "./AlarmTypeDialog";
import { AlarmTypeRow, AlarmTypeColumns, AlarmTypeService } from "../../ServerTypes/SensorsReport";

export class AlarmTypeGrid extends EntityGrid<AlarmTypeRow> {
    protected override getColumnsKey() { return AlarmTypeColumns.columnsKey; }
    protected override getDialogType() { return AlarmTypeDialog; }
    protected override getRowDefinition() { return AlarmTypeRow; }
    protected override getService() { return AlarmTypeService.baseUrl; }
}
