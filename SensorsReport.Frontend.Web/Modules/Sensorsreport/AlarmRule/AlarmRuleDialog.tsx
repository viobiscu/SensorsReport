import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { AlarmRuleForm, AlarmRuleRow, AlarmRuleService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.AlarmRule.RoleDialog')
export class AlarmRuleDialog<P = {}> extends EntityDialog<AlarmRuleRow, P> {
    protected override getFormKey() { return AlarmRuleForm.formKey; }
    protected override getRowDefinition() { return AlarmRuleRow; }
    protected override getService() { return AlarmRuleService.baseUrl; }
}