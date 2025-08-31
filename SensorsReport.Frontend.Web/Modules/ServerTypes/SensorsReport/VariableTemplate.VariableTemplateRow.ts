import { fieldsProxy } from "@serenity-is/corelib";

export interface VariableTemplateRow {
    Id?: string;
    Name?: string;
}

export abstract class VariableTemplateRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Name';
    static readonly localTextPrefix = 'SensorsReport.VariableTemplate';
    static readonly deletePermission = 'Sensorsreport:Management';
    static readonly insertPermission = 'Sensorsreport:Management';
    static readonly readPermission = 'Sensorsreport:Management';
    static readonly updatePermission = 'Sensorsreport:Management';

    static readonly Fields = fieldsProxy<VariableTemplateRow>();
}