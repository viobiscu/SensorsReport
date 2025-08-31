import { fieldsProxy } from "@serenity-is/corelib";

export interface EmailTemplateRow {
    Id?: string;
    Subject?: string;
    Body?: string;
}

export abstract class EmailTemplateRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Subject';
    static readonly localTextPrefix = 'SensorsReport.EmailTemplate';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<EmailTemplateRow>();
}