import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { AlarmRuleRow } from "./AlarmRule.AlarmRuleRow";

export namespace AlarmRuleService {
    export const baseUrl = 'SensorsReport/AlarmRule';

    export declare function Create(request: SaveRequest<AlarmRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<AlarmRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<AlarmRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<AlarmRuleRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<AlarmRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<AlarmRuleRow>>;

    export const Methods = {
        Create: "SensorsReport/AlarmRule/Create",
        Update: "SensorsReport/AlarmRule/Update",
        Delete: "SensorsReport/AlarmRule/Delete",
        Retrieve: "SensorsReport/AlarmRule/Retrieve",
        List: "SensorsReport/AlarmRule/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>AlarmRuleService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}