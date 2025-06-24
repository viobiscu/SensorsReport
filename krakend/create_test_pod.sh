#!/bin/bash

POD_NAME="test-pod"
HOST_DIR=$(pwd)
MOUNT_PATH="/mnt"

echo "[INFO] Starting minikube mount of $HOST_DIR to $MOUNT_PATH ..."
(mount $HOST_DIR:$MOUNT_PATH &) >/dev/null 2>&1

sleep 3  # Give mount some time to start

echo "[INFO] Deleting old pod if it exists..."
kubectl delete pod $POD_NAME --ignore-not-found

echo "[INFO] Creating pod manifest..."
cat <<EOF > test-pod.yaml
apiVersion: v1
kind: Pod
metadata:
  name: $POD_NAME
spec:
  containers:
    - name: $POD_NAME
      image: python:3.10-slim
      command: ["/bin/bash", "-c"]
      args:
        - apt update && apt install -y curl && pip install requests pyjwt && tail -f /dev/null
      volumeMounts:
        - name: host-volume
          mountPath: /xxx
  volumes:
    - name: host-volume
      hostPath:
        path: "$MOUNT_PATH"
        type: Directory
  restartPolicy: Never
EOF

echo "[INFO] Applying pod manifest..."
kubectl apply -f test-pod.yaml

echo "[INFO] Waiting for pod to be ready..."
kubectl wait --for=condition=Ready pod/$POD_NAME --timeout=60s

echo "[INFO] Pod status:"
kubectl get pod $POD_NAME -o wide
