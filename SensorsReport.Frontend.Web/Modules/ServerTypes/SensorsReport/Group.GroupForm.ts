import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface GroupForm {
    Id: StringEditor;
    Name: StringEditor;
    Users: StringEditor;
}

export class GroupForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.GroupForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!GroupForm.init)  {
            GroupForm.init = true;

            var w0 = StringEditor;

            initFormType(GroupForm, [
                'Id', w0,
                'Name', w0,
                'Users', w0
            ]);
        }
    }
}