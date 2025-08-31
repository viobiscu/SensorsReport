import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { GroupForm, GroupRow, GroupService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.Group.RoleDialog')
export class GroupDialog<P = {}> extends EntityDialog<GroupRow, P> {
    protected override getFormKey() { return GroupForm.formKey; }
    protected override getRowDefinition() { return GroupRow; }
    protected override getService() { return GroupService.baseUrl; }
}