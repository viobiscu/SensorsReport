import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { AlarmForm, AlarmRow, AlarmService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.Alarm.RoleDialog')
export class AlarmDialog<P = {}> extends EntityDialog<AlarmRow, P> {
    protected override getFormKey() { return AlarmForm.formKey; }
    protected override getRowDefinition() { return AlarmRow; }
    protected override getService() { return AlarmService.baseUrl; }
}