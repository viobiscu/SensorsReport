import { StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface ApiKeyForm {
    Id: StringEditor;
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
                'Id', w0,
                'TenantId', w0,
                'ApiKey', w0
            ]);
        }
    }
}