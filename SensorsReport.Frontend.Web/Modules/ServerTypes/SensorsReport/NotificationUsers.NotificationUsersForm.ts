import { StringEditor, BooleanEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface NotificationUsersForm {
    Id: StringEditor;
    Name: StringEditor;
    Enable: BooleanEditor;
    Users: StringEditor;
    Groups: StringEditor;
    Notification: StringEditor;
}

export class NotificationUsersForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.NotificationUsersForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!NotificationUsersForm.init)  {
            NotificationUsersForm.init = true;

            var w0 = StringEditor;
            var w1 = BooleanEditor;

            initFormType(NotificationUsersForm, [
                'Id', w0,
                'Name', w0,
                'Enable', w1,
                'Users', w0,
                'Groups', w0,
                'Notification', w0
            ]);
        }
    }
}