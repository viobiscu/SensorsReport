docker build -f Sensors.Report.Audit.API/Dockerfile -t viobiscu/sensors-report-audit-api .

kubectl rollout restart deployment sensors-report-audit-api

