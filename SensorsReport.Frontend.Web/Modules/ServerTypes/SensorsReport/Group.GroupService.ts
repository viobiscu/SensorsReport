import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { GroupRow } from "./Group.GroupRow";

export namespace GroupService {
    export const baseUrl = 'SensorsReport/Group';

    export declare function Create(request: SaveRequest<GroupRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<GroupRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<GroupRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<GroupRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<GroupRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<GroupRow>>;

    export const Methods = {
        Create: "SensorsReport/Group/Create",
        Update: "SensorsReport/Group/Update",
        Delete: "SensorsReport/Group/Delete",
        Retrieve: "SensorsReport/Group/Retrieve",
        List: "SensorsReport/Group/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>GroupService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}