import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { SmsTemplateRow } from "./SmsTemplate.SmsTemplateRow";

export interface SmsTemplateColumns {
    Id: Column<SmsTemplateRow>;
    Name: Column<SmsTemplateRow>;
    Message: Column<SmsTemplateRow>;
}

export class SmsTemplateColumns extends ColumnsBase<SmsTemplateRow> {
    static readonly columnsKey = 'SensorsReport.SmsTemplate';
    static readonly Fields = fieldsProxy<SmsTemplateColumns>();
}