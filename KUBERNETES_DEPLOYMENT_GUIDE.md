# SensorsReport Kubernetes Deployment Guide

This guide provides step-by-step instructions for deploying the SensorsReport platform on Kubernetes using Flux GitOps. The platform consists of multiple microservices, each with its own deployment configuration.

## 📋 Prerequisites

### Infrastructure Requirements
- **Kubernetes cluster** (v1.24+)
- **kubectl** configured to access your cluster
- **Flux CLI** (v2.0+) installed
- **Git repository** access for GitOps workflow
- **Container registry** access (images are hosted on Docker Hub under `viobiscu/`)

### External Dependencies
- **Keycloak** for authentication (https://keycloak.sensorsreport.net)
- **FIWARE Orion Context Broker** for IoT data management
- **RabbitMQ** for message processing
- **PostgreSQL/MongoDB** for persistent storage
- **QuantumLeap** for time-series data

### Resource Requirements
- **Minimum**: 4 CPU cores, 8GB RAM, 50GB storage
- **Recommended**: 8 CPU cores, 16GB RAM, 100GB storage
- **Production**: 16+ CPU cores, 32GB+ RAM, 500GB+ storage

## 🚀 Quick Start Deployment

### 1. Install Flux on Your Cluster

```bash
# Install Flux CLI (if not already installed)
curl -s https://fluxcd.io/install.sh | sudo bash

# Bootstrap Flux in your cluster
flux bootstrap github \
  --owner=<your-github-username> \
  --repository=<your-repo-name> \
  --branch=main \
  --path=./clusters/my-cluster \
  --personal
```

### 2. Deploy Core Infrastructure

Deploy external dependencies first:

```bash
# Create namespace
kubectl create namespace sensorsreport

# Deploy RabbitMQ
kubectl apply -f https://github.com/rabbitmq/cluster-operator/releases/latest/download/cluster-operator.yml

# Deploy MongoDB (for notification storage)
kubectl apply -f - <<EOF
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mongodb
  namespace: sensorsreport
spec:
  replicas: 1
  selector:
    matchLabels:
      app: mongodb
  template:
    metadata:
      labels:
        app: mongodb
    spec:
      containers:
      - name: mongodb
        image: mongo:7
        ports:
        - containerPort: 27017
        env:
        - name: MONGO_INITDB_ROOT_USERNAME
          value: "admin"
        - name: MONGO_INITDB_ROOT_PASSWORD
          value: "password"
---
apiVersion: v1
kind: Service
metadata:
  name: mongodb
  namespace: sensorsreport
spec:
  selector:
    app: mongodb
  ports:
  - port: 27017
    targetPort: 27017
EOF
```

### 3. Deploy SensorsReport Platform

```bash
# Clone the repository
git clone <your-sensorsreport-repo>
cd SensorsReport

# Apply all Flux configurations
kubectl apply -f flux/kustomization.yaml

# Or deploy individual services:
kubectl apply -f flux/sensors-report-alarm-api/
kubectl apply -f flux/sensors-report-provision-api/
kubectl apply -f flux/sensors-report-frontend-web/
# ... continue for other services
```

## 📦 Service-by-Service Deployment

### Core API Services

#### 1. Alarm API
```bash
# Deploy Alarm API
kubectl apply -f SensorsReport.Alarm.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Alarm.API/flux/service.yaml

# Verify deployment
kubectl get pods -l app=sensors-report-alarm-api
kubectl logs -l app=sensors-report-alarm-api
```

#### 2. Business Broker API (Central Orchestration)
```bash
# Deploy Business Broker API
kubectl apply -f SensorsReport.Business.Broker.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Business.Broker.API/flux/service.yaml

# Check health
kubectl port-forward svc/sensors-report-business-broker-api 8080:80
curl http://localhost:8080/health
```

#### 3. Provision API
```bash
# Deploy Provision API
kubectl apply -f SensorsReport.Provision.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Provision.API/flux/service.yaml
```

#### 4. Webhook API
```bash
# Deploy Webhook API
kubectl apply -f SensorsReport.Webhook.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Webhook.API/flux/service.yaml
```

#### 5. Notification Services
```bash
# Email API
kubectl apply -f SensorsReport.Email.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Email.API/flux/service.yaml

# SMS API
kubectl apply -f SensorsReport.SMS.API/flux/deployment.yaml
kubectl apply -f SensorsReport.SMS.API/flux/service.yaml

# Audit API
kubectl apply -f SensorsReport.Audit.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Audit.API/flux/service.yaml
```

### Rule Processing Services

#### 6. Rule APIs and Consumers
```bash
# Alarm Rule API & Consumer
kubectl apply -f SensorsReport.AlarmRule.API/flux/deployment.yaml
kubectl apply -f SensorsReport.AlarmRule.API/flux/service.yaml
kubectl apply -f SensorsReport.AlarmRule.Consumer/flux/deployment.yaml
kubectl apply -f SensorsReport.AlarmRule.Consumer/flux/service.yaml

# Log Rule API & Consumer
kubectl apply -f SensorsReport.LogRule.API/flux/deployment.yaml
kubectl apply -f SensorsReport.LogRule.API/flux/service.yaml
kubectl apply -f SensorsReport.LogRule.Consumer/flux/deployment.yaml
kubectl apply -f SensorsReport.LogRule.Consumer/flux/service.yaml

# Notification Rule API & Consumer
kubectl apply -f SensorsReport.NotificationRule.API/flux/deployment.yaml
kubectl apply -f SensorsReport.NotificationRule.API/flux/service.yaml
kubectl apply -f SensorsReport.NotificationRule.Consumer/flux/deployment.yaml
kubectl apply -f SensorsReport.NotificationRule.Consumer/flux/service.yaml
```

### Frontend Applications

#### 7. Frontend Web Application
```bash
# Deploy database first
kubectl apply -f SensorsReport.Frontend.Web/flux/database.yaml

# Wait for database to be ready
kubectl wait --for=condition=ready pod -l app=frontend-mssql --timeout=300s

# Deploy application
kubectl apply -f SensorsReport.Frontend.Web/flux/deployment.yaml
kubectl apply -f SensorsReport.Frontend.Web/flux/service.yaml

# Create ingress for external access
kubectl apply -f SensorsReport.Frontend.Web/flux/ingress.yaml
```

#### 8. Explorer Dashboard
```bash
# Deploy Explorer components
kubectl apply -f flux/sensors-report-explorer/frontend-deployment.yaml
kubectl apply -f flux/sensors-report-explorer/frontend-service.yaml
kubectl apply -f flux/sensors-report-explorer/backend-deployment.yaml
kubectl apply -f flux/sensors-report-explorer/backend-service.yaml
kubectl apply -f flux/sensors-report-explorer/deployment.yaml
kubectl apply -f flux/sensors-report-explorer/service.yaml
```

### Data Processing Services

#### 9. MQTT to Orion Integration
```bash
# Deploy MQTT to Orion service
kubectl apply -f flux/sensors-report-mqtt-to-orion/deployment.yaml
kubectl apply -f flux/sensors-report-mqtt-to-orion/service.yaml
```

#### 10. Workflow API
```bash
# Deploy Workflow API
kubectl apply -f Sensors-Report-Workflow.API/flux/deployment.yaml
kubectl apply -f Sensors-Report-Workflow.API/flux/service.yaml
```

### API Gateway and Documentation

#### 11. Swagger API Gateway
```bash
# Deploy Swagger API with RBAC
kubectl apply -f SensorsReport.Swagger.API/flux/swagger-rbac.yaml
kubectl apply -f SensorsReport.Swagger.API/flux/deployment.yaml
kubectl apply -f SensorsReport.Swagger.API/flux/service.yaml
```

## 🔧 Configuration and Secrets

### Required Secrets

Create necessary secrets before deployment:

```bash
# Keycloak Client Secret
kubectl create secret generic keycloak-secret \
  --from-literal=client-secret="your-keycloak-client-secret"

# Database Connection Strings
kubectl create secret generic database-secrets \
  --from-literal=mssql-connection="Server=frontend-mssql;Database=frontenddb;User ID=sa;Password=kqI0@pKBLQl9PFSbRN;TrustServerCertificate=True;" \
  --from-literal=mongodb-connection="mongodb://admin:password@mongodb:27017"

# SMTP Configuration (for email notifications)
kubectl create secret generic smtp-secret \
  --from-literal=smtp-host="your-smtp-server" \
  --from-literal=smtp-port="587" \
  --from-literal=smtp-username="your-username" \
  --from-literal=smtp-password="your-password"

# SMS Gateway Configuration
kubectl create secret generic sms-secret \
  --from-literal=gateway-url="http://raspberry-pi-sms-gateway:8000" \
  --from-literal=gateway-token="your-sms-gateway-token"
```

### Environment Configuration

Update ConfigMaps for environment-specific settings:

```bash
# Update Keycloak URLs
kubectl patch configmap sensors-report-frontend-openid-cm \
  --patch='{"data":{"SR_OpenIdSettings__ExternalProviders__Keycloak__Issuer":"https://your-keycloak.domain.com/realms/sr"}}'

# Update Orion-LD URLs
kubectl create configmap orion-config \
  --from-literal=orion-url="http://orion-ld-broker:1026" \
  --from-literal=quantumleap-url="http://quantumleap:8668"
```

## 🧪 Testing and Verification

### Health Checks

```bash
# Check all deployments
kubectl get deployments -A

# Check pod status
kubectl get pods -A | grep sensors-report

# Check services
kubectl get services -A | grep sensors-report
```

### Service Health Endpoints

```bash
# Test individual service health
kubectl port-forward svc/sensors-report-alarm-api 8080:80
curl http://localhost:8080/health

kubectl port-forward svc/sensors-report-provision-api 8081:80
curl http://localhost:8081/health

kubectl port-forward svc/sensors-report-frontend-web 8082:80
curl http://localhost:8082/health
```

### End-to-End Testing

```bash
# Test Frontend Web Application
kubectl port-forward svc/sensors-report-frontend-web 8080:80
# Open browser to http://localhost:8080

# Test API Gateway
kubectl port-forward svc/sensors-report-swagger-api 8081:80
# Open browser to http://localhost:8081/swagger

# Test Explorer Dashboard
kubectl port-forward svc/sensors-report-explorer 8082:80
# Open browser to http://localhost:8082
```

### Database Connectivity

```bash
# Test SQL Server connection
kubectl exec -it deployment/frontend-mssql -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'kqI0@pKBLQl9PFSbRN' -Q "SELECT @@VERSION"

# Test MongoDB connection
kubectl exec -it deployment/mongodb -- mongosh --username admin --password password --eval "db.adminCommand('hello')"
```

## ⚠️ Common Pitfalls and Troubleshooting

### 1. Image Pull Issues

**Problem**: Pods stuck in `ImagePullBackOff` status
```bash
# Check image availability
docker pull viobiscu/sensors-report-alarm-api:latest

# Update image pull policy
kubectl patch deployment sensors-report-alarm-api -p '{"spec":{"template":{"spec":{"containers":[{"name":"sensors-report-alarm-api","imagePullPolicy":"IfNotPresent"}]}}}}'
```

### 2. Database Connection Issues

**Problem**: Services can't connect to databases
```bash
# Check database pods
kubectl get pods -l app=frontend-mssql
kubectl logs deployment/frontend-mssql

# Test network connectivity
kubectl run test-pod --image=busybox --rm -it -- nslookup frontend-mssql
```

### 3. Keycloak Authentication Issues

**Problem**: Authentication failures
```bash
# Verify Keycloak configuration
kubectl get configmap sensors-report-frontend-openid-cm -o yaml

# Check secret values
kubectl get secret sensors-report-frontend-openid-secret -o yaml

# Test Keycloak connectivity
kubectl run test-pod --image=curlimages/curl --rm -it -- curl -k https://keycloak.sensorsreport.net/realms/sr/.well-known/openid_configuration
```

### 4. Resource Constraints

**Problem**: Pods being killed due to resource limits
```bash
# Check resource usage
kubectl top pods
kubectl describe pod <pod-name>

# Adjust resource limits
kubectl patch deployment sensors-report-alarm-api -p '{"spec":{"template":{"spec":{"containers":[{"name":"sensors-report-alarm-api","resources":{"limits":{"memory":"1Gi","cpu":"1000m"}}}]}}}}'
```

### 5. Service Discovery Issues

**Problem**: Services can't communicate with each other
```bash
# Check service endpoints
kubectl get endpoints

# Test service resolution
kubectl run test-pod --image=busybox --rm -it -- nslookup sensors-report-alarm-api

# Check network policies
kubectl get networkpolicies
```

### 6. Persistent Volume Issues

**Problem**: Database data not persisting
```bash
# Check PV and PVC status
kubectl get pv,pvc

# Check storage class
kubectl get storageclass

# Verify volume mounts
kubectl describe pod <database-pod-name>
```

## 🔄 Rolling Updates and Maintenance

### Update Application Images

```bash
# Update specific service
kubectl set image deployment/sensors-report-alarm-api sensors-report-alarm-api=viobiscu/sensors-report-alarm-api:v2.0.0

# Monitor rollout
kubectl rollout status deployment/sensors-report-alarm-api

# Rollback if needed
kubectl rollout undo deployment/sensors-report-alarm-api
```

### Backup and Restore

```bash
# Backup databases
kubectl exec deployment/frontend-mssql -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'kqI0@pKBLQl9PFSbRN' -Q "BACKUP DATABASE frontenddb TO DISK = '/var/opt/mssql/backup/frontenddb.bak'"

# Backup MongoDB
kubectl exec deployment/mongodb -- mongodump --username admin --password password --out /data/backup
```

## 📊 Monitoring and Logging

### Enable Monitoring

```bash
# Deploy Prometheus and Grafana (if not already present)
kubectl apply -f https://raw.githubusercontent.com/prometheus-operator/prometheus-operator/main/bundle.yaml

# Create ServiceMonitor for SensorsReport services
kubectl apply -f - <<EOF
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: sensorsreport-metrics
spec:
  selector:
    matchLabels:
      app: sensors-report
  endpoints:
  - port: http
    path: /metrics
EOF
```

### Centralized Logging

```bash
# Check application logs
kubectl logs -l app=sensors-report-alarm-api --tail=100

# Follow logs in real-time
kubectl logs -f deployment/sensors-report-frontend-web

# Aggregate logs from all services
kubectl logs -l component=sensorsreport --all-containers=true
```

## 🚨 Production Considerations

### Security Hardening

1. **Use non-root containers**
2. **Enable Pod Security Standards**
3. **Configure Network Policies**
4. **Use proper RBAC**
5. **Enable audit logging**
6. **Scan images for vulnerabilities**

### High Availability

1. **Run multiple replicas**
2. **Use anti-affinity rules**
3. **Configure health checks**
4. **Use horizontal pod autoscaling**
5. **Implement circuit breakers**

### Performance Optimization

1. **Configure resource requests/limits**
2. **Use connection pooling**
3. **Enable compression**
4. **Optimize database queries**
5. **Implement caching strategies**

## 📞 Support and Troubleshooting

### Getting Help

1. **Check service logs**: `kubectl logs deployment/<service-name>`
2. **Verify configurations**: Review ConfigMaps and Secrets
3. **Test connectivity**: Use debug pods for network testing
4. **Monitor resources**: Use `kubectl top` commands
5. **Check events**: `kubectl get events --sort-by=.metadata.creationTimestamp`

### Useful Commands

```bash
# Get overall cluster status
kubectl get all -A

# Describe problematic resources
kubectl describe pod <pod-name>
kubectl describe deployment <deployment-name>

# Check resource quotas
kubectl describe quota
kubectl describe limitranges

# Debug networking
kubectl run debug --image=nicolaka/netshoot --rm -it -- bash
```

---

This deployment guide provides comprehensive instructions for deploying the SensorsReport platform on Kubernetes. Follow the steps carefully and refer to the troubleshooting section for common issues. For production deployments, ensure you implement proper security, monitoring, and backup procedures.
