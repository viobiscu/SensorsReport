import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { NotificationUsersRow } from "./NotificationUsers.NotificationUsersRow";

export namespace NotificationUsersService {
    export const baseUrl = 'SensorsReport/NotificationUsers';

    export declare function Create(request: SaveRequest<NotificationUsersRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<NotificationUsersRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<NotificationUsersRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<NotificationUsersRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<NotificationUsersRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<NotificationUsersRow>>;

    export const Methods = {
        Create: "SensorsReport/NotificationUsers/Create",
        Update: "SensorsReport/NotificationUsers/Update",
        Delete: "SensorsReport/NotificationUsers/Delete",
        Retrieve: "SensorsReport/NotificationUsers/Retrieve",
        List: "SensorsReport/NotificationUsers/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>NotificationUsersService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}