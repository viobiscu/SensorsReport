import { Decorators, EditorUtils, EntityDialog } from "@serenity-is/corelib";
import { VariableTemplateForm, VariableTemplateRow, VariableTemplateService } from "../../ServerTypes/SensorsReport";
import { DialogUtils } from "@serenity-is/extensions";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.VariableTemplate.RoleDialog')
export class VariableTemplateDialog<P = {}> extends EntityDialog<VariableTemplateRow, P> {
    protected override getFormKey() { return VariableTemplateForm.formKey; }
    protected override getRowDefinition() { return VariableTemplateRow; }
    protected override getService() { return VariableTemplateService.baseUrl; }

    protected override updateInterface() {
        super.updateInterface();
        this.readOnly = true;
    }
}