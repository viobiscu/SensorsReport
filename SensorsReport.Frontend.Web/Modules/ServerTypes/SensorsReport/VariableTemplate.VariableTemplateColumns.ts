import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { VariableTemplateRow } from "./VariableTemplate.VariableTemplateRow";

export interface VariableTemplateColumns {
    Id: Column<VariableTemplateRow>;
    Name: Column<VariableTemplateRow>;
}

export class VariableTemplateColumns extends ColumnsBase<VariableTemplateRow> {
    static readonly columnsKey = 'SensorsReport.VariableTemplate';
    static readonly Fields = fieldsProxy<VariableTemplateColumns>();
}