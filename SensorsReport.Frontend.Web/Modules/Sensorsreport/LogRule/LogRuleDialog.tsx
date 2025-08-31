import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { LogRuleForm, LogRuleRow, LogRuleService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.LogRule.RoleDialog')
export class LogRuleDialog<P = {}> extends EntityDialog<LogRuleRow, P> {
    protected override getFormKey() { return LogRuleForm.formKey; }
    protected override getRowDefinition() { return LogRuleRow; }
    protected override getService() { return LogRuleService.baseUrl; }
}