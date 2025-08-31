import { PropertyModel } from "./PropertyModel";

export interface MeasuredModel<number> {
    value?: number;
    unit?: PropertyModel<string>;
    observedAt?: string;
}