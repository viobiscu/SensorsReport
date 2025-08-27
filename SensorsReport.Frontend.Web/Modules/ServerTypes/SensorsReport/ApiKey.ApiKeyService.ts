import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { ApiKeyRow } from "./ApiKey.ApiKeyRow";

export namespace ApiKeyService {
    export const baseUrl = 'SensorsReport/ApiKey';

    export declare function Create(request: SaveRequest<ApiKeyRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<ApiKeyRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<ApiKeyRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<ApiKeyRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<ApiKeyRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<ApiKeyRow>>;

    export const Methods = {
        Create: "SensorsReport/ApiKey/Create",
        Update: "SensorsReport/ApiKey/Update",
        Delete: "SensorsReport/ApiKey/Delete",
        Retrieve: "SensorsReport/ApiKey/Retrieve",
        List: "SensorsReport/ApiKey/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>ApiKeyService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}