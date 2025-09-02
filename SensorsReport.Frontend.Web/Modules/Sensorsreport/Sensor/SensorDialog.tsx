import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { SensorForm, SensorRow, SensorService } from "../../ServerTypes/SensorsReport";

@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.Sensor.RoleDialog')
export class SensorDialog<P = {}> extends EntityDialog<SensorRow, P> {
    protected override getFormKey() { return SensorForm.formKey; }
    protected override getRowDefinition() { return SensorRow; }
    protected override getService() { return SensorService.baseUrl; }
}