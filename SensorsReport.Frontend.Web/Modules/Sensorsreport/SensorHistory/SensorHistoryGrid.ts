import { EntityGrid } from "@serenity-is/corelib";
import { SensorHistoryRow, SensorHistoryColumns, SensorHistoryService } from "../../ServerTypes/SensorsReport";
import { SensorHistoryDialog } from "./SensorHistoryDialog";

export class SensorHistoryGrid<P = {}> extends EntityGrid<SensorHistoryRow, P> {
    protected getColumnsKey() { return SensorHistoryColumns.columnsKey; }
    protected getDialogType() { return SensorHistoryDialog; }
    protected getRowDefinition() { return SensorHistoryRow; }
    protected getService() { return SensorHistoryService.baseUrl; }
}
