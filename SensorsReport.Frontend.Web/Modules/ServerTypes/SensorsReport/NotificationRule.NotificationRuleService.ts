import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { NotificationRuleRow } from "./NotificationRule.NotificationRuleRow";

export namespace NotificationRuleService {
    export const baseUrl = 'SensorsReport/NotificationRule';

    export declare function Create(request: SaveRequest<NotificationRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<NotificationRuleRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<NotificationRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<NotificationRuleRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<NotificationRuleRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<NotificationRuleRow>>;

    export const Methods = {
        Create: "SensorsReport/NotificationRule/Create",
        Update: "SensorsReport/NotificationRule/Update",
        Delete: "SensorsReport/NotificationRule/Delete",
        Retrieve: "SensorsReport/NotificationRule/Retrieve",
        List: "SensorsReport/NotificationRule/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>NotificationRuleService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}