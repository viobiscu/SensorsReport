import { StringEditor, DecimalEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface AlarmRuleForm {
    Id: StringEditor;
    Name: StringEditor;
    Unit: StringEditor;
    Low: DecimalEditor;
    PreLow: DecimalEditor;
    PreHigh: DecimalEditor;
    High: DecimalEditor;
}

export class AlarmRuleForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.AlarmRuleForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!AlarmRuleForm.init)  {
            AlarmRuleForm.init = true;

            var w0 = StringEditor;
            var w1 = DecimalEditor;

            initFormType(AlarmRuleForm, [
                'Id', w0,
                'Name', w0,
                'Unit', w0,
                'Low', w1,
                'PreLow', w1,
                'PreHigh', w1,
                'High', w1
            ]);
        }
    }
}