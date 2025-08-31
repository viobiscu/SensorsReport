import { AlarmStaticsModel } from "./AlarmStaticsModel";
import { SensorStaticsModel } from "./SensorStaticsModel";

export interface DashboardPageModel {
    SensorStaticsModel?: SensorStaticsModel;
    AlarmStaticsModel?: AlarmStaticsModel;
}