import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { UserForm, UserRow, UserService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.User.RoleDialog')
export class UserDialog<P = {}> extends EntityDialog<UserRow, P> {
    protected override getFormKey() { return UserForm.formKey; }
    protected override getRowDefinition() { return UserRow; }
    protected override getService() { return UserService.baseUrl; }
}