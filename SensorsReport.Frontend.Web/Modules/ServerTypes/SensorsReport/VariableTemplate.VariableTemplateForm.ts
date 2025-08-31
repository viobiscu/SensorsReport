import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface VariableTemplateForm {
    Id: StringEditor;
    Name: StringEditor;
}

export class VariableTemplateForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.VariableTemplateForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!VariableTemplateForm.init)  {
            VariableTemplateForm.init = true;

            var w0 = StringEditor;

            initFormType(VariableTemplateForm, [
                'Id', w0,
                'Name', w0
            ]);
        }
    }
}