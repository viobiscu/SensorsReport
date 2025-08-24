import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface ApiKeyForm {
    TenantId: StringEditor;
    ApiKey: StringEditor;
}

export class ApiKeyForm extends PrefixedContext {
    static readonly formKey = 'SensorsReport.ApiKeyForm';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!ApiKeyForm.init)  {
            ApiKeyForm.init = true;

            var w0 = StringEditor;

            initFormType(ApiKeyForm, [
                'TenantId', w0,
                'ApiKey', w0
            ]);
        }
    }
}