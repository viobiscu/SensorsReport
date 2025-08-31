import { StringEditor, DecimalEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface AlarmForm {
    Id: StringEditor;
    Description: StringEditor;
    Status: StringEditor;
    Severity: StringEditor;
    Monitors: StringEditor;
    Threshold: DecimalEditor;
    Condition: StringEditor;
    MeasuredValue: StringEditor;
}

export class AlarmForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.AlarmForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!AlarmForm.init)  {
            AlarmForm.init = true;

            var w0 = StringEditor;
            var w1 = DecimalEditor;

            initFormType(AlarmForm, [
                'Id', w0,
                'Description', w0,
                'Status', w0,
                'Severity', w0,
                'Monitors', w0,
                'Threshold', w1,
                'Condition', w0,
                'MeasuredValue', w0
            ]);
        }
    }
}