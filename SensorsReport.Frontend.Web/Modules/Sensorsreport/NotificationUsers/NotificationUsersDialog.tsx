import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { NotificationUsersRow, NotificationUsersForm, NotificationUsersService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.NotificationUsers.RoleDialog')
export class NotificationUsersDialog<P = {}> extends EntityDialog<NotificationUsersRow, P> {
    protected override getFormKey() { return NotificationUsersForm.formKey; }
    protected override getRowDefinition() { return NotificationUsersRow; }
    protected override getService() { return NotificationUsersService.baseUrl; }
}