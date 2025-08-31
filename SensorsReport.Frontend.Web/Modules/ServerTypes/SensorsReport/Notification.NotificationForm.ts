import { StringEditor, BooleanEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface NotificationForm {
    Id: StringEditor;
    Name: StringEditor;
    Enable: BooleanEditor;
    NotificationRule: StringEditor;
    NotificationUser: StringEditor;
    SMS: StringEditor;
    Email: StringEditor;
    Monitors: StringEditor;
}

export class NotificationForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.NotificationForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!NotificationForm.init)  {
            NotificationForm.init = true;

            var w0 = StringEditor;
            var w1 = BooleanEditor;

            initFormType(NotificationForm, [
                'Id', w0,
                'Name', w0,
                'Enable', w1,
                'NotificationRule', w0,
                'NotificationUser', w0,
                'SMS', w0,
                'Email', w0,
                'Monitors', w0
            ]);
        }
    }
}