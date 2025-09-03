import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { SensorHistoryRow } from "./SensorHistory.SensorHistoryRow";

export namespace SensorHistoryService {
    export const baseUrl = 'SensorsReport/SensorHistory';

    export declare function Create(request: SaveRequest<SensorHistoryRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<SensorHistoryRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<SensorHistoryRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<SensorHistoryRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<SensorHistoryRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<SensorHistoryRow>>;

    export const Methods = {
        Create: "SensorsReport/SensorHistory/Create",
        Update: "SensorsReport/SensorHistory/Update",
        Delete: "SensorsReport/SensorHistory/Delete",
        Retrieve: "SensorsReport/SensorHistory/Retrieve",
        List: "SensorsReport/SensorHistory/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>SensorHistoryService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}