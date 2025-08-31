import { faIcon, stringFormat } from "@serenity-is/corelib";
import { DashboardPageModel } from "../../ServerTypes/Common/DashboardPageModel";
import { SmallCard } from "./shared/small-card";

export default function pageInit({ model, nwLinkFormat }: { model: DashboardPageModel, nwLinkFormat: string }) {

    const nwLink = (s: string) => stringFormat(nwLinkFormat, s);

    document.getElementById("DashboardContent").append(<>
        <div class="row">
            <SmallCard caption="Sensors" icon={faIcon("microchip")} value={model.SensorStaticsModel.SensorCount} colorClass="bg-primary text-white" />
            <SmallCard caption="Active Sensors" icon={faIcon("check-circle")} value={model.SensorStaticsModel.ActiveSensorCount} colorClass="bg-success text-white" />
            <SmallCard caption="Fault Sensors" icon={faIcon("times-circle")} value={model.SensorStaticsModel.FaultSensors} colorClass="bg-warning text-white" />
            <SmallCard caption="Alerts" icon={faIcon("exclamation-triangle")} value={model.SensorStaticsModel.AlertSensorCount} colorClass="bg-danger text-white" />
        </div>

        <div class="row">
            <SmallCard caption="Alarms" icon={faIcon("bell")} value={model.AlarmStaticsModel.AlarmCount} colClass="col-sm-2" colorClass="bg-info text-white" />
            <SmallCard caption="Pre Low Alarms" icon={faIcon("arrow-down")} value={model.AlarmStaticsModel.PreLowAlarms} colClass="col-sm-2" colorClass="bg-warning text-white" />
            <SmallCard caption="Low Alarms" icon={faIcon("arrow-down")} value={model.AlarmStaticsModel.LowAlarms} colClass="col-sm-2" colorClass="bg-danger text-white" />
            <SmallCard caption="Pre High Alarms" icon={faIcon("arrow-up")} value={model.AlarmStaticsModel.PreHighAlarms} colClass="col-sm-2" colorClass="bg-warning text-white" />
            <SmallCard caption="High Alarms" icon={faIcon("arrow-up")} value={model.AlarmStaticsModel.HighAlarms} colClass="col-sm-2" colorClass="bg-danger text-white" />
            <SmallCard caption="Archived Alarms" icon={faIcon("archive")} value={model.AlarmStaticsModel.ArchivedAlarms} colClass="col-sm-2" colorClass="bg-black text-white" />
        </div>


        <div class="row">
            <section class="col-lg-7">
            </section>

            <section class="col-lg-5">
            </section>
        </div>
    </>)
}