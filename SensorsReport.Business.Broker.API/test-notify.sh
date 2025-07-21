#!/bin/bash

echo "Testing SensorsReportBusinessBroker API - Notify Endpoint"
echo "========================================================"
echo 

curl -v -X POST http://localhost:5000/v1/notify \
  -H "Content-Type: application/ld+json" \
  -H "NGSILD-Tenant: test-tenant" \
  -H "Fiware-ServicePath: /test" \
  -d '{
    "subscriptionId": "urn:ngsi-ld:Subscription:test123",
    "data": [
      {
        "id": "urn:ngsi-ld:Sensor:test1",
        "type": "Sensor",
        "temperature": {
          "type": "Property",
          "value": 25.5
        },
        "humidity": {
          "type": "Property",
          "value": 60
        }
      }
    ]
  }'

echo
echo "Test complete."
