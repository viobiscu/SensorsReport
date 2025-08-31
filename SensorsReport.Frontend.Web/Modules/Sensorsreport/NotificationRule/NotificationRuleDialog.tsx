import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { NotificationRuleRow, NotificationRuleForm, NotificationRuleService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.NotificationRule.RoleDialog')
export class NotificationRuleDialog<P = {}> extends EntityDialog<NotificationRuleRow, P> {
    protected override getFormKey() { return NotificationRuleForm.formKey; }
    protected override getRowDefinition() { return NotificationRuleRow; }
    protected override getService() { return NotificationRuleService.baseUrl; }
}