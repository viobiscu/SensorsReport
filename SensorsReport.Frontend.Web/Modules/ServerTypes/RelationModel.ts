import { PropertyModel } from "./PropertyModel";

export interface RelationModel<string> {
    object?: string[];
    enable?: PropertyModel<boolean>;
    monitoredAttribute?: PropertyModel<string>;
}