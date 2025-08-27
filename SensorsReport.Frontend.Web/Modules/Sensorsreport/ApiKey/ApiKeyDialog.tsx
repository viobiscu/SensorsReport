import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { ApiKeyForm, ApiKeyRow, ApiKeyService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.ApiKey.RoleDialog')
export class ApiKeyDialog<P = {}> extends EntityDialog<ApiKeyRow, P> {
    protected override getFormKey() { return ApiKeyForm.formKey; }
    protected override getRowDefinition() { return ApiKeyRow; }
    protected override getService() { return ApiKeyService.baseUrl; }
}