import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { EmailTemplateRow } from "./EmailTemplate.EmailTemplateRow";

export namespace EmailTemplateService {
    export const baseUrl = 'SensorsReport/EmailTemplate';

    export declare function Create(request: SaveRequest<EmailTemplateRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<EmailTemplateRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<EmailTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<EmailTemplateRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<EmailTemplateRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<EmailTemplateRow>>;

    export const Methods = {
        Create: "SensorsReport/EmailTemplate/Create",
        Update: "SensorsReport/EmailTemplate/Update",
        Delete: "SensorsReport/EmailTemplate/Delete",
        Retrieve: "SensorsReport/EmailTemplate/Retrieve",
        List: "SensorsReport/EmailTemplate/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>EmailTemplateService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}