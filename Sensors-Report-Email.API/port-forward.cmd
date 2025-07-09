START "MongoDB Proxy" kubectl port-forward notification-mongodb-0 27017:27017 --namespace=mongodb
START "RabbitMQ Proxy" kubectl port-forward rabbitmq-default-server-0 5672:5672 --namespace=default
