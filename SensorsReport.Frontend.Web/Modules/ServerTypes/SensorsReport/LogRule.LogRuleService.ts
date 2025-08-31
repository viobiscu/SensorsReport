import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { LogRuleRow } from "./LogRule.LogRuleRow";

export namespace LogRuleService {
    export const baseUrl = 'SensorsReport/LogRule';

    export declare function Create(request: SaveRequest<LogRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<LogRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<LogRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<LogRuleRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<LogRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<LogRuleRow>>;

    export const Methods = {
        Create: "SensorsReport/LogRule/Create",
        Update: "SensorsReport/LogRule/Update",
        Delete: "SensorsReport/LogRule/Delete",
        Retrieve: "SensorsReport/LogRule/Retrieve",
        List: "SensorsReport/LogRule/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>LogRuleService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}