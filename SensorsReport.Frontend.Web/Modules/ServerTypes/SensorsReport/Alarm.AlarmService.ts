import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { AlarmRow } from "./Alarm.AlarmRow";

export namespace AlarmService {
    export const baseUrl = 'SensorsReport/Alarm';

    export declare function Create(request: SaveRequest<AlarmRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<AlarmRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<AlarmRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<AlarmRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<AlarmRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<AlarmRow>>;

    export const Methods = {
        Create: "SensorsReport/Alarm/Create",
        Update: "SensorsReport/Alarm/Update",
        Delete: "SensorsReport/Alarm/Delete",
        Retrieve: "SensorsReport/Alarm/Retrieve",
        List: "SensorsReport/Alarm/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>AlarmService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}