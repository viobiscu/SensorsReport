import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { EmailTemplateRow, EmailTemplateForm, EmailTemplateService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.EmailTemplate.RoleDialog')
export class EmailTemplateDialog<P = {}> extends EntityDialog<EmailTemplateRow, P> {
    protected override getFormKey() { return EmailTemplateForm.formKey; }
    protected override getRowDefinition() { return EmailTemplateRow; }
    protected override getService() { return EmailTemplateService.baseUrl; }
}