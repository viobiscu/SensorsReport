import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { AlarmTypeForm, AlarmTypeRow, AlarmTypeService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.AlarmType.RoleDialog')
export class AlarmTypeDialog<P = {}> extends EntityDialog<AlarmTypeRow, P> {
    protected override getFormKey() { return AlarmTypeForm.formKey; }
    protected override getRowDefinition() { return AlarmTypeRow; }
    protected override getService() { return AlarmTypeService.baseUrl; }
}