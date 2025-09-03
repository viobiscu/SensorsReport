import { BaseDialog, Decorators, WidgetProps } from "@serenity-is/corelib";
import { CategoryScale, Chart, Legend, LinearScale, LineController, LineElement, PointElement } from "chart.js";
import { SensorHistoryService, SensorHistoryRow } from "../../ServerTypes/SensorsReport";
Chart.register(LineController, LineElement, CategoryScale, Legend, LinearScale, PointElement);

@Decorators.resizable()
@Decorators.maximizable()
@Decorators.registerClass('SensorsReport.Frontend.SensorsReport.Sensor.RoleDialog')
export class SensorDialog<P = { sensorId?: string }> extends BaseDialog<P> {
    declare private canvas: HTMLCanvasElement;

    constructor(props: WidgetProps<P>) {
        super(props);
    }

    protected onDialogOpen() {
        super.onDialogOpen();

        SensorHistoryService.List({
            EqualityFilter: { SensorId: this.props["sensorId"] },
            Sort: [SensorHistoryRow.Fields.ObservedAt + " desc"],
            Take: 50
        }, response => {
            var entities = response.Entities as SensorHistoryRow[];
            if (entities.length === 0) {
                this.canvas.parentElement!.innerHTML = "<div class='alert alert-info m-2'>No data found</div>";
                return;
            }

            var groups = entities.reduce((acc, curr) => {
                if (!acc[curr.PropertyKey]) {
                    acc[curr.PropertyKey] = [];
                }
                acc[curr.PropertyKey].push(curr);
                return acc;
            }, {} as { [key: string]: SensorHistoryRow[] });

            new Chart(this.canvas, {
                type: "line",
                data: {
                    labels: entities.map(x => new Date(x.ObservedAt).toLocaleString()).reverse(),
                    datasets: Object.keys(groups).map((key, idx) => ({
                        label: key,
                        backgroundColor: `hsl(${(idx * 137.5) % 360}, 50%, 75%)`,
                        borderColor: `hsl(${(idx * 137.5) % 360}, 50%, 50%)`,
                        fill: false,
                        data: groups[key].map(x => x.Value).reverse()
                    }))
                }
            })
        });
    }

    protected renderContents(): any {
        return (<canvas id={`${this.idPrefix}Chart`} ref={el => this.canvas = el}></canvas>);
    }

    protected getDialogOptions() {
        var opt = super.getDialogOptions();
        opt.title = `Sensor History - ${this.props["sensorId"]}`;
        opt.modal = false;
        opt.backdrop = true;
        return opt;
    }
}