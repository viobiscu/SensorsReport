import { StringEditor, TextAreaEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface SmsTemplateForm {
    Id: StringEditor;
    Name: StringEditor;
    Message: TextAreaEditor;
}

export class SmsTemplateForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.SmsTemplateForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!SmsTemplateForm.init)  {
            SmsTemplateForm.init = true;

            var w0 = StringEditor;
            var w1 = TextAreaEditor;

            initFormType(SmsTemplateForm, [
                'Id', w0,
                'Name', w0,
                'Message', w1
            ]);
        }
    }
}