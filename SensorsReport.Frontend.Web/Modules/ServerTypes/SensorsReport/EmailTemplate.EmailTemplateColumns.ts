import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { EmailTemplateRow } from "./EmailTemplate.EmailTemplateRow";

export interface EmailTemplateColumns {
    Id: Column<EmailTemplateRow>;
    Subject: Column<EmailTemplateRow>;
    Body: Column<EmailTemplateRow>;
}

export class EmailTemplateColumns extends ColumnsBase<EmailTemplateRow> {
    static readonly columnsKey = 'SensorsReport.EmailTemplate';
    static readonly Fields = fieldsProxy<EmailTemplateColumns>();
}