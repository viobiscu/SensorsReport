import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { ApiKeyRow } from "./ApiKey.ApiKeyRow";

export interface ApiKeyColumns {
    Id: Column<ApiKeyRow>;
    TenantId: Column<ApiKeyRow>;
    ApiKey: Column<ApiKeyRow>;
}

export class ApiKeyColumns extends ColumnsBase<ApiKeyRow> {
    static readonly columnsKey = 'SensorsReport.ApiKey';
    static readonly Fields = fieldsProxy<ApiKeyColumns>();
}