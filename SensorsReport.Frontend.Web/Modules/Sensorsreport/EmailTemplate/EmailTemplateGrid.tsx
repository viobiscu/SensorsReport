import { EntityGrid } from "@serenity-is/corelib";
import { EmailTemplateDialog } from "./EmailTemplateDialog";
import { EmailTemplateRow, EmailTemplateColumns, EmailTemplateService } from "../../ServerTypes/SensorsReport";

export class EmailTemplateGrid extends EntityGrid<EmailTemplateRow> {
    protected override getColumnsKey() { return EmailTemplateColumns.columnsKey; }
    protected override getDialogType() { return EmailTemplateDialog; }
    protected override getRowDefinition() { return EmailTemplateRow; }
    protected override getService() { return EmailTemplateService.baseUrl; }
}
