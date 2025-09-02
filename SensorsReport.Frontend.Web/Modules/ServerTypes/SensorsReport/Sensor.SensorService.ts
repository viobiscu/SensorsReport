import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { SensorRow } from "./Sensor.SensorRow";

export namespace SensorService {
    export const baseUrl = 'SensorsReport/Sensor';

    export declare function Create(request: SaveRequest<SensorRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<SensorRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<SensorRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<SensorRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<SensorRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<SensorRow>>;

    export const Methods = {
        Create: "SensorsReport/Sensor/Create",
        Update: "SensorsReport/Sensor/Update",
        Delete: "SensorsReport/Sensor/Delete",
        Retrieve: "SensorsReport/Sensor/Retrieve",
        List: "SensorsReport/Sensor/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>SensorService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}