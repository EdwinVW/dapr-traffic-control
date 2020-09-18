helm repo add bitnami https://charts.bitnami.com/bitnami
helm install redis bitnami/redis
kubectl get secret --namespace default redis -o jsonpath="{.data.redis-password}" > encoded.b64
certutil -decode encoded.b64 password.txt
type .\password.txt
del encoded.b64
del password.txt