kubectl create configmap krakend --from-file=krakend.json=./krakend.json --dry-run=client -o yaml | kubectl replace -f -
kubectl rollout restart deployment krakend

echo ============configmap===============================
kubectl get configmap krakend
echo ============deployment==============================
kubectl get deployment krakend
echo ============service=================================
kubectl get service krakend