import { StringEditor, DecimalEditor, IntegerEditor, BooleanEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface LogRuleForm {
    Id: StringEditor;
    Name: StringEditor;
    Unit: StringEditor;
    Low: DecimalEditor;
    High: DecimalEditor;
    ConsecutiveHit: IntegerEditor;
    Enabled: BooleanEditor;
}

export class LogRuleForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.LogRuleForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!LogRuleForm.init)  {
            LogRuleForm.init = true;

            var w0 = StringEditor;
            var w1 = DecimalEditor;
            var w2 = IntegerEditor;
            var w3 = BooleanEditor;

            initFormType(LogRuleForm, [
                'Id', w0,
                'Name', w0,
                'Unit', w0,
                'Low', w1,
                'High', w1,
                'ConsecutiveHit', w2,
                'Enabled', w3
            ]);
        }
    }
}