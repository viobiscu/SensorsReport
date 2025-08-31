import { StringEditor, BooleanEditor, IntegerEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface NotificationRuleForm {
    Id: StringEditor;
    Name: StringEditor;
    Enable: BooleanEditor;
    ConsecutiveHits: IntegerEditor;
    RepeatAfter: IntegerEditor;
    NotifyIfClose: BooleanEditor;
    NotifyIfAcknowledged: BooleanEditor;
    RepeatIfAcknowledged: IntegerEditor;
    NotifyIfTimeOut: IntegerEditor;
    NotificationChannel: StringEditor;
}

export class NotificationRuleForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.NotificationRuleForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!NotificationRuleForm.init)  {
            NotificationRuleForm.init = true;

            var w0 = StringEditor;
            var w1 = BooleanEditor;
            var w2 = IntegerEditor;

            initFormType(NotificationRuleForm, [
                'Id', w0,
                'Name', w0,
                'Enable', w1,
                'ConsecutiveHits', w2,
                'RepeatAfter', w2,
                'NotifyIfClose', w1,
                'NotifyIfAcknowledged', w1,
                'RepeatIfAcknowledged', w2,
                'NotifyIfTimeOut', w2,
                'NotificationChannel', w0
            ]);
        }
    }
}