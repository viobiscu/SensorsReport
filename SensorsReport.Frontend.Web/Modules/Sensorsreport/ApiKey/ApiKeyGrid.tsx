import { EntityGrid } from "@serenity-is/corelib";
import { ApiKeyDialog } from "./ApiKeyDialog";
import { ApiKeyRow, ApiKeyColumns, ApiKeyService } from "../../ServerTypes/SensorsReport";

export class ApiKeyGrid extends EntityGrid<ApiKeyRow> {
    protected override getColumnsKey() { return ApiKeyColumns.columnsKey; }
    protected override getDialogType() { return ApiKeyDialog; }
    protected override getRowDefinition() { return ApiKeyRow; }
    protected override getService() { return ApiKeyService.baseUrl; }
}
