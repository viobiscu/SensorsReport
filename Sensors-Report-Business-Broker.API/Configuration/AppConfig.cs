using System;

namespace SensorsReportBusinessBroker.API.Configuration
{
    public class AppConfig
    {
        // Orion Context Broker configuration
        public string OrionHost { get; set; } = Environment.GetEnvironmentVariable("SR_BB_ORION_HOST") ?? "orion.sensorsreport.net";
        public int OrionPort { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("SR_BB_ORION_PORT"), out int orionPort) ? orionPort : 31026;
        
        // Audit service configuration
        public string AuditUrl { get; set; } = Environment.GetEnvironmentVariable("SR_BB_AUDIT_URL") ?? "localhost";
        public int AuditPort { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("SR_BB_AUDIT_PORT"), out int auditPort) ? auditPort : 80;
        
        // Keycloak configuration
        public string KeycloakRealm { get; set; } = Environment.GetEnvironmentVariable("SR_BB_KEYCLOAK_RELM") ?? "sr";
        public string KeycloakClientId { get; set; } = Environment.GetEnvironmentVariable("SR_BB_KEYCLOAK_CLIENTID") ?? "ContextBroker";
        public string KeycloakClientSecret { get; set; } = Environment.GetEnvironmentVariable("SR_BB_KEYCLOAK_CLIENTSECRET") ?? "AELYK4tusYazvIDIvw0meQZiSnGMnVJP";

        // Derived properties
        public string OrionUrl => $"http://{OrionHost}:{OrionPort}";
        public string AuditServiceUrl => $"http://{AuditUrl}:{AuditPort}";
    }
}