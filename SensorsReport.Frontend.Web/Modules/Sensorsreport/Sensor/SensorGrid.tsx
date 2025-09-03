import { confirmDialog, EntityGrid, Fluent, htmlEncode, stringFormat } from "@serenity-is/corelib";
import { FormatterContext } from "@serenity-is/sleekgrid";
import { SensorDialog } from "./SensorDialog";
import { SensorRow, SensorColumns, SensorService } from "../../ServerTypes/SensorsReport";

export class SensorGrid extends EntityGrid<SensorRow> {
    protected override getColumnsKey() { return SensorColumns.columnsKey; }
    protected override getDialogType() { return SensorDialog; }
    protected override getRowDefinition() { return SensorRow; }
    protected override getService() { return SensorService.baseUrl; }

    protected override getButtons() {
        var buttons = super.getButtons();
        buttons = buttons.filter(s => s.cssClass != "add-button");

        return buttons;
    }

    protected override getColumns() {
        var columns = super.getColumns();
        var cols = new SensorColumns(columns);

        cols.Id.format = ctx => (<a href="#" class="chart-details">{ctx.value}</a>);

        cols.RH0.format = (ctx) => {
            return <>
                <span title={ctx.item.RH0_ObservedAt}>{ctx.item.RH0} {this.toUnit(ctx.item.RH0_Unit)}</span>
            </>;
        }
        cols.RH0_Unit.visible = false;
        cols.RH0_ObservedAt.visible = false;
        cols.RH0_Name.format = this.formatEmptyText;
        cols.RH0_Status.format = this.formatStatus;

        cols.T0.format = (ctx) => {
            return <>
                <span title={ctx.item.T0_ObservedAt}>{ctx.item.T0} {this.toUnit(ctx.item.T0_Unit)}</span>
            </>;
        }
        cols.T0_Unit.visible = false;
        cols.T0_ObservedAt.visible = false;
        cols.T0_Name.format = this.formatEmptyText;
        cols.T0_Status.format = this.formatStatus;

        return columns;
    }

    formatEmptyText(ctx: FormatterContext<SensorRow>) {
        var value = (ctx.value as string)?.trim();
        if (value?.length > 0)
            return <>{value}</>;

        return <>-</>;
    }

    formatStatus(ctx: FormatterContext<SensorRow>) {
        var klass = "fa ";
        if (ctx.value?.toLocaleLowerCase() == "operational")
            klass += "fa-check text-success";
        else if (ctx.value?.toLocaleLowerCase() == "faulty")
            klass += "fa-times text-danger";
        else
            klass += "fa-question text-muted";

        return <>
            <i class={klass}></i>
        </>
    }

    toUnit(unit: string): string {
        switch (unit) {
            case 'cel':
                return '°C';
            case 'fah':
                return '°F';
            case 'kel':
                return 'K';
            case 'per':
                return '%';
            case 'lux':
                return 'lux';
            case 'vol':
                return 'V';
            case 'db':
                return 'dB';
        }

        return unit;
    }

    protected getItemCssClass(item: SensorRow, index: number): string {
        let klass: string = "";

        if (item.T0_Status?.toLocaleLowerCase() == "operational")
            klass += " text-success";
        else if (item.T0_Status?.toLocaleLowerCase() == "faulty")
            klass += " text-danger";

        return klass.trim() || null;
    }

    protected onClick(e: Event, row: number, cell: number): void {

        // let base grid handle clicks for its edit links
        super.onClick(e, row, cell);

        // if base grid already handled, we shouldn"t handle it again
        if (Fluent.isDefaultPrevented(e)) {
            return;
        }

        // get reference to current item
        var item = this.itemAt(row);

        // get reference to clicked element
        var target = e.target as HTMLElement;

        if (target.classList.contains("chart-details")) {
            e.preventDefault();
            var dialog = new SensorDialog({
                sensorId: item.Id
            });

            dialog.dialogOpen();
        }
    }
}
