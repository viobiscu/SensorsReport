using System;

namespace SensorsReportAudit
{
    public class AuditConfig
    {
        // QuantumLeap settings
        public string QuantumLeapHost { get; set; } = string.Empty;
        public string QuantumLeapPort { get; set; } = string.Empty;
        
        // Keycloak settings
        public string KeycloakUrl { get; set; } = string.Empty;
        public string KeycloakPort { get; set; } = string.Empty;
        public string KeycloakRealm { get; set; } = string.Empty;
        public string KeycloakClientId { get; set; } = string.Empty;
        public string KeycloakClientSecret { get; set; } = string.Empty;
        
        // Helper properties
        public string QuantumLeapBaseUrl => $"http://{QuantumLeapHost}:{QuantumLeapPort}";
        public string KeycloakBaseUrl => $"http://{KeycloakUrl}:{KeycloakPort}";
        public string KeycloakTokenEndpoint => $"{KeycloakBaseUrl}/realms/{KeycloakRealm}/protocol/openid-connect/token";
        public string KeycloakIntrospectEndpoint => $"{KeycloakBaseUrl}/realms/{KeycloakRealm}/protocol/openid-connect/token/introspect";
        
        public static AuditConfig FromEnvironment()
        {
            return new AuditConfig
            {
                QuantumLeapHost = Environment.GetEnvironmentVariable("SR_AUDIT_QUANTUMLEAP_HOST") ?? "localhost",
                QuantumLeapPort = Environment.GetEnvironmentVariable("SR_AUDIT_QUANTUMLEAP_PORT") ?? "8668",
                KeycloakUrl = Environment.GetEnvironmentVariable("SR_AUDIT_KEYCLOAK_URL") ?? "localhost",
                KeycloakPort = Environment.GetEnvironmentVariable("SR_AUDIT_KEYCLOAK_PORT") ?? "8080",
                KeycloakRealm = Environment.GetEnvironmentVariable("SR_AUDIT_KEYCLOAK_REALM") ?? "master",
                KeycloakClientId = Environment.GetEnvironmentVariable("SR_AUDIT_KEYCLOAK_CLIENTID") ?? "",
                KeycloakClientSecret = Environment.GetEnvironmentVariable("SR_AUDIT_KEYCLOAK_CLIENTSECRET") ?? ""
            };
        }
    }
}