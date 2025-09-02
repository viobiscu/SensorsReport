import { StringEditor, DecimalEditor, DateEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface SensorForm {
    Id: StringEditor;
    T0_Name: StringEditor;
    T0: DecimalEditor;
    T0_Unit: StringEditor;
    T0_ObservedAt: DateEditor;
    T0_Status: StringEditor;
    RH0_Name: StringEditor;
    RH0: DecimalEditor;
    RH0_Unit: StringEditor;
    RH0_ObservedAt: DateEditor;
    RH0_Status: StringEditor;
}

export class SensorForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.SensorForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!SensorForm.init)  {
            SensorForm.init = true;

            var w0 = StringEditor;
            var w1 = DecimalEditor;
            var w2 = DateEditor;

            initFormType(SensorForm, [
                'Id', w0,
                'T0_Name', w0,
                'T0', w1,
                'T0_Unit', w0,
                'T0_ObservedAt', w2,
                'T0_Status', w0,
                'RH0_Name', w0,
                'RH0', w1,
                'RH0_Unit', w0,
                'RH0_ObservedAt', w2,
                'RH0_Status', w0
            ]);
        }
    }
}