import { StringEditor, HtmlContentEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface EmailTemplateForm {
    Id: StringEditor;
    Subject: StringEditor;
    Body: HtmlContentEditor;
}

export class EmailTemplateForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.EmailTemplateForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!EmailTemplateForm.init)  {
            EmailTemplateForm.init = true;

            var w0 = StringEditor;
            var w1 = HtmlContentEditor;

            initFormType(EmailTemplateForm, [
                'Id', w0,
                'Subject', w0,
                'Body', w1
            ]);
        }
    }
}