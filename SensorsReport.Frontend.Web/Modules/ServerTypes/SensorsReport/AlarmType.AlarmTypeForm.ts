import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface AlarmTypeForm {
    Id: StringEditor;
    Name: StringEditor;
    Description: StringEditor;
    Style: StringEditor;
}

export class AlarmTypeForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.AlarmTypeForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!AlarmTypeForm.init)  {
            AlarmTypeForm.init = true;

            var w0 = StringEditor;

            initFormType(AlarmTypeForm, [
                'Id', w0,
                'Name', w0,
                'Description', w0,
                'Style', w0
            ]);
        }
    }
}