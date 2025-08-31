import { EntityGrid } from "@serenity-is/corelib";
import { SmsTemplateDialog } from "./SmsTemplateDialog";
import { SmsTemplateRow, SmsTemplateColumns, SmsTemplateService } from "../../ServerTypes/SensorsReport";

export class SmsTemplateGrid extends EntityGrid<SmsTemplateRow> {
    protected override getColumnsKey() { return SmsTemplateColumns.columnsKey; }
    protected override getDialogType() { return SmsTemplateDialog; }
    protected override getRowDefinition() { return SmsTemplateRow; }
    protected override getService() { return SmsTemplateService.baseUrl; }
}
