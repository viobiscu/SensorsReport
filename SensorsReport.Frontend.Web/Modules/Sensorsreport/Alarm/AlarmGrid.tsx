import { EntityGrid } from "@serenity-is/corelib";
import { AlarmDialog } from "./AlarmDialog";
import { AlarmRow, AlarmColumns, AlarmService } from "../../ServerTypes/SensorsReport";

export class AlarmGrid extends EntityGrid<AlarmRow> {
    protected override getColumnsKey() { return AlarmColumns.columnsKey; }
    protected override getDialogType() { return AlarmDialog; }
    protected override getRowDefinition() { return AlarmRow; }
    protected override getService() { return AlarmService.baseUrl; }
}
