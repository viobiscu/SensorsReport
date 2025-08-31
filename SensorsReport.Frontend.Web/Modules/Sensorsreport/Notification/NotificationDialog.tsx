import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { NotificationRow, NotificationForm, NotificationService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.Notification.RoleDialog')
export class NotificationDialog<P = {}> extends EntityDialog<NotificationRow, P> {
    protected override getFormKey() { return NotificationForm.formKey; }
    protected override getRowDefinition() { return NotificationRow; }
    protected override getService() { return NotificationService.baseUrl; }
}