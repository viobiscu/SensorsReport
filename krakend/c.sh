helm uninstall api-gateway

#rm krakend-config.yaml


kubectl delete configmap krakend
kubectl delete deployment krakend
kubectl delete service krakend



kubectl create configmap krakend --from-file=krakend.json=./krakend.json
kubectl apply -f krakend-deployment.yaml
kubectl apply -f krakend-service.yaml

echo ============configmap===============================
kubectl get configmap krakend
echo ============deployment==============================
kubectl get deployment krakend
echo ============service=================================
kubectl get service krakend
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "
echo " "

