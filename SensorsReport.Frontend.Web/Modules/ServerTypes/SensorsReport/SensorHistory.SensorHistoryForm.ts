import { StringEditor, DateEditor, DecimalEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface SensorHistoryForm {
    Id: StringEditor;
    Tenant: StringEditor;
    SensorId: StringEditor;
    PropertyKey: StringEditor;
    MetadataKey: StringEditor;
    ObservedAt: DateEditor;
    Value: DecimalEditor;
    Unit: StringEditor;
    CreatedAt: DateEditor;
}

export class SensorHistoryForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.SensorHistory';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!SensorHistoryForm.init)  {
            SensorHistoryForm.init = true;

            var w0 = StringEditor;
            var w1 = DateEditor;
            var w2 = DecimalEditor;

            initFormType(SensorHistoryForm, [
                'Id', w0,
                'Tenant', w0,
                'SensorId', w0,
                'PropertyKey', w0,
                'MetadataKey', w0,
                'ObservedAt', w1,
                'Value', w2,
                'Unit', w0,
                'CreatedAt', w1
            ]);
        }
    }
}