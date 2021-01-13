helm repo add bitnami https://charts.bitnami.com/bitnami
helm install redis bitnami/redis
kubectl get secret --namespace default redis -o jsonpath="{.data.redis-password}" > ./encoded.b64
certutil -decode ./encoded.b64 ./password.txt > $null
Get-Content ./password.txt
Remove-Item ./encoded.b64
Remove-Item ./password.txt