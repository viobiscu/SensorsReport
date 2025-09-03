import { Decorators, EntityDialog } from "@serenity-is/corelib";
import { SensorHistoryRow, SensorHistoryForm, SensorHistoryService } from "../../ServerTypes/SensorsReport";


@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.SensorHistoryDialog')
export class SensorHistoryDialog<P = {}> extends EntityDialog<SensorHistoryRow, P> {
    protected getFormKey() { return SensorHistoryForm.formKey; }
    protected getRowDefinition() { return SensorHistoryRow; }
    protected getService() { return SensorHistoryService.baseUrl; }
}
