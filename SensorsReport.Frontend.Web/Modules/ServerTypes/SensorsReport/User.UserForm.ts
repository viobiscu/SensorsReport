import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface UserForm {
    Id: StringEditor;
    Username: StringEditor;
    Email: StringEditor;
    FirstName: StringEditor;
    LastName: StringEditor;
    Mobile: StringEditor;
    Language: StringEditor;
}

export class UserForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.UserForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!UserForm.init)  {
            UserForm.init = true;

            var w0 = StringEditor;

            initFormType(UserForm, [
                'Id', w0,
                'Username', w0,
                'Email', w0,
                'FirstName', w0,
                'LastName', w0,
                'Mobile', w0,
                'Language', w0
            ]);
        }
    }
}