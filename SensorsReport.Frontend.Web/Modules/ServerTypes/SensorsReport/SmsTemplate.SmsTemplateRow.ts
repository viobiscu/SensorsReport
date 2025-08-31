import { fieldsProxy } from "@serenity-is/corelib";

export interface SmsTemplateRow {
    Id?: string;
    Name?: string;
    Message?: string;
}

export abstract class SmsTemplateRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.SmsTemplate';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<SmsTemplateRow>();
}