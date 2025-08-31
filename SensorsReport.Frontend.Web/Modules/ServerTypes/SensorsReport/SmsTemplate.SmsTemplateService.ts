import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { SmsTemplateRow } from "./SmsTemplate.SmsTemplateRow";

export namespace SmsTemplateService {
    export const baseUrl = 'SensorsReport/SmsTemplate';

    export declare function Create(request: SaveRequest<SmsTemplateRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<SmsTemplateRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<SmsTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<SmsTemplateRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<SmsTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<SmsTemplateRow>>;

    export const Methods = {
        Create: "SensorsReport/SmsTemplate/Create",
        Update: "SensorsReport/SmsTemplate/Update",
        Delete: "SensorsReport/SmsTemplate/Delete",
        Retrieve: "SensorsReport/SmsTemplate/Retrieve",
        List: "SensorsReport/SmsTemplate/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>SmsTemplateService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}