# SensorsReport Security Best Practices Guide

This comprehensive security guide provides best practices, implementation examples, and testing procedures for securing the SensorsReport platform across all microservices, infrastructure components, and deployment environments.

## 🔒 Table of Contents

1. [Container Security](#container-security)
2. [Kubernetes Security](#kubernetes-security)
3. [API Security](#api-security)
4. [Authentication & Authorization](#authentication--authorization)
5. [Data Protection](#data-protection)
6. [Network Security](#network-security)
7. [Secrets Management](#secrets-management)
8. [Monitoring & Auditing](#monitoring--auditing)
9. [Security Testing](#security-testing)
10. [Common Pitfalls](#common-pitfalls)

## 🐳 Container Security

### Non-Root User Implementation

**Problem**: Running containers as root increases security risks.

**Solution**: Configure all containers to run as non-root users.

#### Example Implementation for .NET APIs

**Dockerfile (SensorsReport.Alarm.API)**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Create non-root user
RUN groupadd -r sensorsreport && useradd -r -g sensorsreport sensorsreport

# Set proper permissions
RUN mkdir -p /app && chown -R sensorsreport:sensorsreport /app
WORKDIR /app

# Switch to non-root user
USER sensorsreport

EXPOSE 8080
ENV ASPNETCORE_URLS="http://+:8080"

# Rest of Dockerfile...
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... existing build steps ...

FROM base AS final
COPY --from=build --chown=sensorsreport:sensorsreport /app/publish .
ENTRYPOINT ["dotnet", "SensorsReport.Alarm.API.dll"]
```

**Kubernetes Deployment Security Context**:
```yaml
# SensorsReport.Alarm.API/flux/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sensors-report-alarm-api
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        runAsGroup: 1000
        fsGroup: 1000
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: sensors-report-alarm-api
        image: viobiscu/sensors-report-alarm-api:latest
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000
          capabilities:
            drop:
            - ALL
        volumeMounts:
        - name: tmp-volume
          mountPath: /tmp
        - name: app-logs
          mountPath: /app/logs
      volumes:
      - name: tmp-volume
        emptyDir: {}
      - name: app-logs
        emptyDir: {}
```

### Image Security Scanning

**Best Practice**: Scan all container images for vulnerabilities.

```bash
# Add to CI/CD pipeline
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  -v $HOME/Library/Caches:/root/.cache/ \
  aquasec/trivy image viobiscu/sensors-report-alarm-api:latest

# For critical vulnerabilities only
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image --severity HIGH,CRITICAL viobiscu/sensors-report-alarm-api:latest
```

### Multi-stage Build Security

**Example for Python Services (SMS Gateway)**:
```dockerfile
# SensorReport.PI.SMS.Gateway/Dockerfile
FROM python:3.11-slim AS base

# Security updates
RUN apt-get update && apt-get upgrade -y && \
    apt-get install -y --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r smsgateway && useradd -r -g smsgateway smsgateway

FROM base AS dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

FROM base AS final
COPY --from=dependencies /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=dependencies /usr/local/bin /usr/local/bin

# Copy application with proper ownership
COPY --chown=smsgateway:smsgateway . /app
WORKDIR /app

# Switch to non-root user
USER smsgateway

EXPOSE 8000
CMD ["python", "sms_gateway.py"]
```

## ⚙️ Kubernetes Security

### Pod Security Standards

**Implementation**: Apply Pod Security Standards across all deployments.

```yaml
# Create Pod Security Policy
apiVersion: v1
kind: Namespace
metadata:
  name: sensorsreport
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

### Network Policies

**Example**: Restrict communication between services.

```yaml
# Network policy for Frontend Web
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: frontend-web-netpol
  namespace: sensorsreport
spec:
  podSelector:
    matchLabels:
      app: sensors-report-frontend-web
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: nginx-ingress
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: frontend-mssql
    ports:
    - protocol: TCP
      port: 1433
  - to:
    - podSelector:
        matchLabels:
          app: sensors-report-business-broker-api
    ports:
    - protocol: TCP
      port: 80
  # Allow DNS resolution
  - to: []
    ports:
    - protocol: UDP
      port: 53
```

### RBAC Configuration

**Service Account for each service**:
```yaml
# SensorsReport.Alarm.API/flux/rbac.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: alarm-api-sa
  namespace: sensorsreport
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: sensorsreport
  name: alarm-api-role
rules:
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list"]
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "list"]
  resourceNames: ["sensors-report-alarm-api-*"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: alarm-api-binding
  namespace: sensorsreport
subjects:
- kind: ServiceAccount
  name: alarm-api-sa
  namespace: sensorsreport
roleRef:
  kind: Role
  name: alarm-api-role
  apiGroup: rbac.authorization.k8s.io
```

### Resource Limits and Security

```yaml
# Enhanced deployment with security constraints
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sensors-report-business-broker-api
spec:
  template:
    spec:
      serviceAccountName: business-broker-sa
      automountServiceAccountToken: false
      securityContext:
        runAsNonRoot: true
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: business-broker-api
        resources:
          limits:
            cpu: "500m"
            memory: "512Mi"
            ephemeral-storage: "1Gi"
          requests:
            cpu: "100m"
            memory: "128Mi"
            ephemeral-storage: "500Mi"
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000
          capabilities:
            drop:
            - ALL
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

## 🔐 API Security

### JWT Authentication Implementation

**Example for API Controllers**:
```csharp
// SensorsReport.Alarm.API/Controllers/AlarmController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class AlarmController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin,operator")]
    public async Task<IActionResult> CreateAlarm([FromBody] CreateAlarmRequest request)
    {
        // Validate tenant from JWT claims
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized("Tenant information missing");
        }

        // Implement request validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Input sanitization
        var sanitizedRequest = _sanitizer.Sanitize(request);
        
        var result = await _alarmService.CreateAlarmAsync(sanitizedRequest, tenantId);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "admin,operator,viewer")]
    public async Task<IActionResult> GetAlarms([FromQuery] AlarmQueryRequest query)
    {
        // Implement query parameter validation
        if (query.PageSize > 100)
        {
            return BadRequest("Page size cannot exceed 100");
        }

        var tenantId = User.FindFirst("tenant_id")?.Value;
        var result = await _alarmService.GetAlarmsAsync(query, tenantId);
        return Ok(result);
    }
}
```

### API Rate Limiting

**Implementation using ASP.NET Core**:
```csharp
// Program.cs for API services
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Per-user rate limiting
    options.AddPolicy("PerUserPolicy", context =>
    {
        var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Global rate limiting
    options.AddFixedWindowLimiter("GlobalPolicy", options =>
    {
        options.PermitLimit = 1000;
        options.Window = TimeSpan.FromMinutes(1);
    });
});

app.UseRateLimiter();
```

### Input Validation and Sanitization

**Model Validation Example**:
```csharp
// SensorsReport.Api.Core/Models/CreateAlarmRequest.cs
public class CreateAlarmRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Invalid characters in alarm name")]
    public string AlarmName { get; set; }

    [Required]
    [Range(typeof(DateTime), "2020-01-01", "2030-12-31")]
    public DateTime Timestamp { get; set; }

    [Required]
    [Range(-50, 100)]
    public decimal TemperatureValue { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}

// Custom validation attribute
public class SanitizedStringAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is string str)
        {
            // Remove potentially dangerous characters
            var sanitized = Regex.Replace(str, @"[<>""']", "");
            return sanitized == str;
        }
        return true;
    }
}
```

### CORS Configuration

```csharp
// Secure CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("SensorsReportPolicy", policy =>
    {
        policy.WithOrigins(
            "https://sensorsreport.yourdomain.com",
            "https://admin.sensorsreport.yourdomain.com"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Authorization", "Content-Type", "X-Tenant-ID")
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
```

## 🔑 Authentication & Authorization

### Keycloak Integration Security

**Enhanced Keycloak Configuration**:
```csharp
// SensorsReport.Frontend.Web/Initialization/Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = ".SensorsReportAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.Secure = true; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://keycloak.sensorsreport.net/realms/sr";
        options.Audience = "SensorsReport";
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
}
```

### Multi-Tenant Security

**Tenant Isolation Implementation**:
```csharp
// SensorsReport.Api.Core/Middleware/TenantValidationMiddleware.cs
public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantValidationMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract tenant from JWT claims
        var tenantClaim = context.User?.FindFirst("tenant_id")?.Value;
        
        // Extract tenant from NGSILD-Tenant header
        var tenantHeader = context.Request.Headers["NGSILD-Tenant"].FirstOrDefault();

        // Validate tenant consistency
        if (!string.IsNullOrEmpty(tenantClaim) && !string.IsNullOrEmpty(tenantHeader))
        {
            if (tenantClaim != tenantHeader)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Tenant mismatch");
                return;
            }
        }

        // Set tenant context
        context.Items["TenantId"] = tenantClaim ?? tenantHeader;
        
        await _next(context);
    }
}
```

### API Key Security

**Secure API Key Management**:
```csharp
// SensorsReport.Frontend.Web/Modules/Sensorsreport/ApiKey/ApiKeyEndpoint.cs
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class ApiKeyController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        // Generate cryptographically secure API key
        var apiKey = GenerateSecureApiKey();
        
        // Hash the API key for storage (never store plain text)
        var hashedKey = BCrypt.Net.BCrypt.HashPassword(apiKey);
        
        // Store with expiration and permissions
        var keyEntity = new ApiKeyEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HashedKey = hashedKey,
            TenantId = GetCurrentTenantId(),
            CreatedBy = User.Identity.Name,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays),
            Permissions = request.Permissions,
            IsActive = true
        };

        await _apiKeyService.CreateAsync(keyEntity);
        
        // Return the plain API key only once
        return Ok(new { ApiKey = apiKey, Id = keyEntity.Id });
    }

    private string GenerateSecureApiKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
```

## 🛡️ Data Protection

### Database Security

**SQL Server Security Configuration**:
```yaml
# SensorsReport.Frontend.Web/flux/database.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend-mssql
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 10001
        fsGroup: 10001
      containers:
      - name: mssql
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
        - name: ACCEPT_EULA
          value: "Y"
        - name: SA_PASSWORD
          valueFrom:
            secretKeyRef:
              name: mssql-secret
              key: SA_PASSWORD
        - name: MSSQL_PID
          value: "Express"
        volumeMounts:
        - name: mssql-data
          mountPath: /var/opt/mssql/data
        - name: mssql-log
          mountPath: /var/opt/mssql/log
        - name: mssql-backup
          mountPath: /var/opt/mssql/backup
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: false # SQL Server requires write access
          capabilities:
            drop:
            - ALL
```

### Data Encryption

**Application-Level Encryption**:
```csharp
// SensorsReport.Api.Core/Services/EncryptionService.cs
public class EncryptionService
{
    private readonly byte[] _key;
    
    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"];
        _key = Convert.FromBase64String(keyString);
    }

    public string EncryptSensitiveData(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        
        swEncrypt.Write(plainText);
        
        var iv = aes.IV;
        var encrypted = msEncrypt.ToArray();
        var result = new byte[iv.Length + encrypted.Length];
        
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
        
        return Convert.ToBase64String(result);
    }
}
```

### Audit Logging Security

**Secure Audit Implementation**:
```csharp
// SensorsReport.Audit/AuditLogger.cs
public class SecureAuditLogger
{
    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            EventType = securityEvent.EventType,
            UserId = securityEvent.UserId,
            TenantId = securityEvent.TenantId,
            IPAddress = HashIPAddress(securityEvent.IPAddress), // Hash PII
            UserAgent = securityEvent.UserAgent,
            EventData = JsonSerializer.Serialize(securityEvent.Data),
            Signature = GenerateSignature(securityEvent) // Tamper detection
        };

        await _auditRepository.CreateAsync(auditEntry);
    }

    private string HashIPAddress(string ipAddress)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress + _salt));
        return Convert.ToBase64String(hash);
    }

    private string GenerateSignature(SecurityEvent securityEvent)
    {
        using var hmac = new HMACSHA256(_signingKey);
        var data = JsonSerializer.Serialize(securityEvent);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
```

## 🌐 Network Security

### TLS/HTTPS Configuration

**Ingress Security Configuration**:
```yaml
# Enhanced ingress with security headers
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: sensorsreport-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    nginx.ingress.kubernetes.io/ssl-protocols: "TLSv1.2 TLSv1.3"
    nginx.ingress.kubernetes.io/ssl-ciphers: "ECDHE-RSA-AES128-GCM-SHA256,ECDHE-RSA-AES256-GCM-SHA384"
    nginx.ingress.kubernetes.io/configuration-snippet: |
      add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
      add_header X-Frame-Options "DENY" always;
      add_header X-Content-Type-Options "nosniff" always;
      add_header X-XSS-Protection "1; mode=block" always;
      add_header Referrer-Policy "strict-origin-when-cross-origin" always;
      add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'" always;
spec:
  tls:
  - hosts:
    - sensorsreport.yourdomain.com
    - admin.sensorsreport.yourdomain.com
    secretName: sensorsreport-tls
  rules:
  - host: admin.sensorsreport.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: sensors-report-frontend-web
            port:
              number: 80
```

### Service Mesh Security (Istio)

**Istio Security Policies**:
```yaml
# Enable mTLS for service-to-service communication
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: sensorsreport
spec:
  mtls:
    mode: STRICT

---
# Authorization policy
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: alarm-api-authz
  namespace: sensorsreport
spec:
  selector:
    matchLabels:
      app: sensors-report-alarm-api
  rules:
  - from:
    - source:
        principals: ["cluster.local/ns/sensorsreport/sa/business-broker-sa"]
    - source:
        principals: ["cluster.local/ns/sensorsreport/sa/frontend-web-sa"]
    to:
    - operation:
        methods: ["GET", "POST", "PUT"]
```

## 🔐 Secrets Management

### Kubernetes Secrets Security

**Encrypted Secrets with External Secrets Operator**:
```yaml
# Install External Secrets Operator
apiVersion: v1
kind: Namespace
metadata:
  name: external-secrets
---
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: vault-secret-store
  namespace: sensorsreport
spec:
  provider:
    vault:
      server: "https://vault.yourdomain.com"
      path: "secret"
      version: "v2"
      auth:
        kubernetes:
          mountPath: "kubernetes"
          role: "sensorsreport-role"

---
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: database-credentials
  namespace: sensorsreport
spec:
  refreshInterval: 5m
  secretStoreRef:
    name: vault-secret-store
    kind: SecretStore
  target:
    name: database-secret
    creationPolicy: Owner
  data:
  - secretKey: connection-string
    remoteRef:
      key: database
      property: connection-string
```

### Secret Rotation

**Automated Secret Rotation Script**:
```bash
#!/bin/bash
# rotate-secrets.sh

set -euo pipefail

NAMESPACE="sensorsreport"
VAULT_ADDR="https://vault.yourdomain.com"

# Function to rotate database password
rotate_db_password() {
    echo "Rotating database password..."
    
    # Generate new password
    NEW_PASSWORD=$(openssl rand -base64 32)
    
    # Update in Vault
    vault kv put secret/database connection-string="Server=frontend-mssql;Database=frontenddb;User ID=sa;Password=${NEW_PASSWORD};TrustServerCertificate=True;"
    
    # Update SQL Server
    kubectl exec deployment/frontend-mssql -n $NAMESPACE -- \
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$OLD_PASSWORD" \
        -Q "ALTER LOGIN sa WITH PASSWORD = '$NEW_PASSWORD'"
    
    # Force secret refresh
    kubectl annotate externalsecret database-credentials -n $NAMESPACE \
        force-sync=$(date +%s) --overwrite
    
    echo "Database password rotated successfully"
}

# Function to rotate API keys
rotate_api_keys() {
    echo "Rotating API keys..."
    
    # Get expired keys
    EXPIRED_KEYS=$(kubectl exec deployment/sensors-report-frontend-web -n $NAMESPACE -- \
        dotnet SensorsReport.Frontend.Web.dll --list-expired-keys)
    
    for key_id in $EXPIRED_KEYS; do
        echo "Rotating key: $key_id"
        kubectl exec deployment/sensors-report-frontend-web -n $NAMESPACE -- \
            dotnet SensorsReport.Frontend.Web.dll --rotate-key $key_id
    done
}

# Main execution
rotate_db_password
rotate_api_keys

echo "Secret rotation completed"
```

## 📊 Monitoring & Auditing

### Security Monitoring

**Falco Security Monitoring**:
```yaml
# Falco rules for SensorsReport
apiVersion: v1
kind: ConfigMap
metadata:
  name: falco-rules
data:
  application_rules.yaml: |
    - rule: Sensitive File Access in SensorsReport
      desc: Detect access to sensitive files in SensorsReport containers
      condition: >
        spawned_process and container and
        (k8s.ns.name = "sensorsreport") and
        (fd.name contains "/etc/passwd" or fd.name contains "/etc/shadow" or
         fd.name contains "appsettings.json" or fd.name contains ".pfx")
      output: >
        Sensitive file access in SensorsReport
        (user=%user.name command=%proc.cmdline file=%fd.name
         container=%container.name image=%container.image.repository)
      priority: WARNING

    - rule: Unexpected Network Connection from SensorsReport
      desc: Detect unexpected outbound connections from SensorsReport containers
      condition: >
        outbound and container and
        (k8s.ns.name = "sensorsreport") and
        not (fd.sip.name contains "sensorsreport" or
             fd.sip.name contains "keycloak" or
             fd.sip.name contains "mongodb" or
             fd.sip.name contains "orion")
      output: >
        Unexpected outbound connection from SensorsReport
        (connection=%fd.name container=%container.name image=%container.image.repository)
      priority: WARNING
```

### Audit Configuration

**Enhanced Audit Logging**:
```csharp
// SensorsReport.Api.Core/Middleware/AuditMiddleware.cs
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditLogger _auditLogger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var auditEvent = new AuditEvent
        {
            RequestId = context.TraceIdentifier,
            UserId = context.User?.Identity?.Name,
            TenantId = context.Items["TenantId"]?.ToString(),
            IPAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"],
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            Timestamp = DateTime.UtcNow
        };

        Exception exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            auditEvent.StatusCode = context.Response.StatusCode;
            auditEvent.Duration = stopwatch.ElapsedMilliseconds;
            auditEvent.Exception = exception?.ToString();

            // Log high-risk events
            if (ShouldAudit(auditEvent))
            {
                await _auditLogger.LogAsync(auditEvent);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldAudit(AuditEvent auditEvent)
    {
        return auditEvent.StatusCode >= 400 || // Error responses
               auditEvent.Method != "GET" || // Mutations
               auditEvent.Path.Contains("admin") || // Admin operations
               auditEvent.Duration > 5000; // Slow requests
    }
}
```

## 🧪 Security Testing

### Automated Security Tests

**Security Test Suite**:
```csharp
// SensorsReport.Security.Tests/SecurityTests.cs
[TestClass]
public class SecurityTests
{
    private readonly HttpClient _client;
    
    [TestMethod]
    public async Task ShouldRequireAuthentication()
    {
        // Test that endpoints require authentication
        var response = await _client.GetAsync("/api/alarms");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldPreventSQLInjection()
    {
        var maliciousInput = "'; DROP TABLE Alarms; --";
        var response = await _client.GetAsync($"/api/alarms?search={maliciousInput}");
        
        // Should not return 500 (SQL error)
        Assert.AreNotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldPreventXSS()
    {
        var xssPayload = "<script>alert('xss')</script>";
        var alarm = new { Name = xssPayload };
        
        var response = await _client.PostAsJsonAsync("/api/alarms", alarm);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task ShouldEnforceTenantIsolation()
    {
        // Test with tenant A token
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _tenantAToken);
        
        var response = await _client.GetAsync("/api/alarms");
        var alarms = await response.Content.ReadFromJsonAsync<Alarm[]>();
        
        // Should only return tenant A's alarms
        Assert.IsTrue(alarms.All(a => a.TenantId == "tenant-a"));
    }

    [TestMethod]
    public async Task ShouldRateLimitRequests()
    {
        // Send 200 requests rapidly
        var tasks = Enumerable.Range(0, 200)
            .Select(_ => _client.GetAsync("/api/alarms"))
            .ToArray();
        
        var responses = await Task.WhenAll(tasks);
        
        // Some should be rate limited
        Assert.IsTrue(responses.Any(r => r.StatusCode == HttpStatusCode.TooManyRequests));
    }
}
```

### Penetration Testing Script

```bash
#!/bin/bash
# security-test.sh

set -euo pipefail

BASE_URL="https://sensorsreport.yourdomain.com"
API_URL="$BASE_URL/api"

echo "Starting security tests for SensorsReport..."

# Test 1: Check for HTTPS enforcement
echo "Testing HTTPS enforcement..."
HTTP_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://sensorsreport.yourdomain.com)
if [[ $HTTP_RESPONSE == "301" || $HTTP_RESPONSE == "302" ]]; then
    echo "✓ HTTPS enforcement working"
else
    echo "✗ HTTPS enforcement failed"
fi

# Test 2: Check security headers
echo "Testing security headers..."
HEADERS=$(curl -s -I $BASE_URL)
if echo "$HEADERS" | grep -q "Strict-Transport-Security"; then
    echo "✓ HSTS header present"
else
    echo "✗ HSTS header missing"
fi

# Test 3: Test authentication bypass
echo "Testing authentication bypass..."
UNAUTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/alarms")
if [[ $UNAUTH_RESPONSE == "401" ]]; then
    echo "✓ Authentication required"
else
    echo "✗ Authentication bypass possible"
fi

# Test 4: SQL Injection test
echo "Testing SQL injection protection..."
SQL_INJECTION="'; DROP TABLE Users; --"
SQL_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/alarms?search=$SQL_INJECTION")
if [[ $SQL_RESPONSE != "500" ]]; then
    echo "✓ SQL injection protection working"
else
    echo "✗ Possible SQL injection vulnerability"
fi

# Test 5: XSS Protection
echo "Testing XSS protection..."
XSS_PAYLOAD="<script>alert('xss')</script>"
XSS_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" \
    -d "{\"name\":\"$XSS_PAYLOAD\"}" \
    "$API_URL/alarms" -w "%{http_code}")
if [[ $XSS_RESPONSE == "400" ]]; then
    echo "✓ XSS protection working"
else
    echo "✗ Possible XSS vulnerability"
fi

# Test 6: Rate limiting
echo "Testing rate limiting..."
for i in {1..110}; do
    curl -s -o /dev/null "$API_URL/health" &
done
wait

RATE_LIMIT_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/health")
if [[ $RATE_LIMIT_RESPONSE == "429" ]]; then
    echo "✓ Rate limiting working"
else
    echo "✓ Rate limiting may not be configured (or limits are high)"
fi

echo "Security testing completed"
```

### Container Security Scanning

```bash
#!/bin/bash
# scan-containers.sh

SERVICES=(
    "sensors-report-alarm-api"
    "sensors-report-business-broker-api"
    "sensors-report-frontend-web"
    "sensors-report-provision-api"
    "sensors-report-webhook-api"
    "sensors-report-email-api"
    "sensors-report-sms-api"
    "sensors-report-audit-api"
)

echo "Scanning SensorsReport container images..."

for service in "${SERVICES[@]}"; do
    echo "Scanning viobiscu/$service:latest..."
    
    # Trivy scan
    trivy image --severity HIGH,CRITICAL "viobiscu/$service:latest"
    
    # Check for non-root user
    docker run --rm --entrypoint="" "viobiscu/$service:latest" whoami | grep -v root || echo "Warning: $service may be running as root"
    
    echo "---"
done

echo "Container security scan completed"
```

## ⚠️ Common Security Pitfalls

### 1. Secrets in Configuration Files

**❌ Wrong**:
```yaml
# Don't do this
env:
- name: DATABASE_PASSWORD
  value: "MyPassword123!"
```

**✅ Correct**:
```yaml
env:
- name: DATABASE_PASSWORD
  valueFrom:
    secretKeyRef:
      name: database-secret
      key: password
```

### 2. Running Containers as Root

**❌ Wrong**:
```dockerfile
# Dockerfile without user specification
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "App.dll"]
```

**✅ Correct**:
```dockerfile
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "App.dll"]
```

### 3. Missing Input Validation

**❌ Wrong**:
```csharp
[HttpPost]
public async Task<IActionResult> CreateAlarm(string name, decimal value)
{
    // Direct database insertion without validation
    var alarm = new Alarm { Name = name, Value = value };
    await _context.Alarms.AddAsync(alarm);
    await _context.SaveChangesAsync();
    return Ok();
}
```

**✅ Correct**:
```csharp
[HttpPost]
public async Task<IActionResult> CreateAlarm([FromBody] CreateAlarmRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    var sanitizedName = _sanitizer.Sanitize(request.Name);
    var alarm = new Alarm 
    { 
        Name = sanitizedName, 
        Value = Math.Max(-50, Math.Min(100, request.Value)) 
    };
    
    await _alarmService.CreateAsync(alarm);
    return Ok();
}
```

### 4. Inadequate Error Handling

**❌ Wrong**:
```csharp
try
{
    var user = await _userService.GetByIdAsync(id);
    return Ok(user);
}
catch (Exception ex)
{
    return BadRequest(ex.Message); // Exposes internal details
}
```

**✅ Correct**:
```csharp
try
{
    var user = await _userService.GetByIdAsync(id);
    return Ok(user);
}
catch (UserNotFoundException)
{
    return NotFound();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error retrieving user {UserId}", id);
    return StatusCode(500, "An error occurred");
}
```

### 5. Missing HTTPS Enforcement

**❌ Wrong**:
```csharp
// No HTTPS enforcement
app.UseAuthentication();
app.UseAuthorization();
```

**✅ Correct**:
```csharp
app.UseHttpsRedirection();
app.UseHsts();
app.UseAuthentication();
app.UseAuthorization();
```

### 6. Weak Session Management

**❌ Wrong**:
```csharp
services.AddAuthentication()
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Too long
        options.Cookie.Secure = false; // Not secure
    });
```

**✅ Correct**:
```csharp
services.AddAuthentication()
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Secure = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });
```

## 🔍 Security Checklist

### Pre-Deployment Security Checklist

- [ ] All containers run as non-root users
- [ ] Secrets are managed through Kubernetes secrets or external secret management
- [ ] Network policies are configured to restrict pod-to-pod communication
- [ ] RBAC is configured with least privilege principles
- [ ] TLS/HTTPS is enforced for all communications
- [ ] Input validation is implemented for all API endpoints
- [ ] Authentication and authorization are properly configured
- [ ] Rate limiting is implemented
- [ ] Security headers are configured
- [ ] Container images are scanned for vulnerabilities
- [ ] Audit logging is enabled
- [ ] Backup and disaster recovery procedures are tested
- [ ] Security monitoring is configured

### Runtime Security Monitoring

- [ ] Monitor for privilege escalation attempts
- [ ] Track unusual network connections
- [ ] Monitor file system changes
- [ ] Alert on authentication failures
- [ ] Monitor resource usage anomalies
- [ ] Track API abuse patterns
- [ ] Monitor for data exfiltration attempts

## 📞 Security Incident Response

### Incident Response Plan

1. **Detection**: Automated alerts from security monitoring
2. **Assessment**: Determine severity and scope
3. **Containment**: Isolate affected systems
4. **Eradication**: Remove threats and vulnerabilities
5. **Recovery**: Restore services safely
6. **Lessons Learned**: Update security measures

### Emergency Contacts

- **Security Team**: security@yourdomain.com
- **Development Team**: dev-team@yourdomain.com
- **Operations Team**: ops@yourdomain.com

---

This security guide provides comprehensive protection for the SensorsReport platform. Regular security reviews, updates, and testing are essential for maintaining a secure environment. Remember that security is an ongoing process, not a one-time implementation.
