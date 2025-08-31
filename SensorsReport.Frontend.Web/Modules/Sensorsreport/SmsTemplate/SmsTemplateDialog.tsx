import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { SmsTemplateRow, SmsTemplateForm, SmsTemplateService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.SmsTemplate.RoleDialog')
export class SmsTemplateDialog<P = {}> extends EntityDialog<SmsTemplateRow, P> {
    protected override getFormKey() { return SmsTemplateForm.formKey; }
    protected override getRowDefinition() { return SmsTemplateRow; }
    protected override getService() { return SmsTemplateService.baseUrl; }
}