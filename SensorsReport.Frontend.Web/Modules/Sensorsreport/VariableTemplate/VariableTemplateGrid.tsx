import { EntityGrid } from "@serenity-is/corelib";
import { VariableTemplateDialog } from "./VariableTemplateDialog";
import { VariableTemplateRow, VariableTemplateColumns, VariableTemplateService } from "../../ServerTypes/SensorsReport";

export class VariableTemplateGrid extends EntityGrid<VariableTemplateRow> {
    protected override getColumnsKey() { return VariableTemplateColumns.columnsKey; }
    protected override getDialogType() { return VariableTemplateDialog; }
    protected override getRowDefinition() { return VariableTemplateRow; }
    protected override getService() { return VariableTemplateService.baseUrl; }

    protected override getButtons() {
        var buttons = super.getButtons();
        buttons = buttons.filter(x => x.cssClass != "add-button");
        return buttons;
    }
}
