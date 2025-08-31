import { RetrieveRequest, RetrieveResponse, ServiceOptions, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { VariableTemplateRow } from "./VariableTemplate.VariableTemplateRow";

export namespace VariableTemplateService {
    export const baseUrl = 'SensorsReport/VariableTemplate';

    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<VariableTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<VariableTemplateRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<VariableTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<VariableTemplateRow>>;

    export const Methods = {
        Retrieve: "SensorsReport/VariableTemplate/Retrieve",
        List: "SensorsReport/VariableTemplate/List"
    } as const;

    [
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>VariableTemplateService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}