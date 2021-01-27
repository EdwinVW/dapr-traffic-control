kubectl apply -f namespace.yaml

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install redis --set password=JOhCGe6Eau bitnami/redis
