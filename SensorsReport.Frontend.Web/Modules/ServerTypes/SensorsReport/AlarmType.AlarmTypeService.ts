import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { AlarmTypeRow } from "./AlarmType.AlarmTypeRow";

export namespace AlarmTypeService {
    export const baseUrl = 'SensorsReport/AlarmType';

    export declare function Create(request: SaveRequest<AlarmTypeRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<AlarmTypeRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<AlarmTypeRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<AlarmTypeRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<AlarmTypeRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<AlarmTypeRow>>;

    export const Methods = {
        Create: "SensorsReport/AlarmType/Create",
        Update: "SensorsReport/AlarmType/Update",
        Delete: "SensorsReport/AlarmType/Delete",
        Retrieve: "SensorsReport/AlarmType/Retrieve",
        List: "SensorsReport/AlarmType/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>AlarmTypeService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}