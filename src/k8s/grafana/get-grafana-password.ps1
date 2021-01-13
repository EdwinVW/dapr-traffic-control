# display Grafana admin password
kubectl get secret --namespace dapr-monitoring grafana -o jsonpath="{.data.admin-password}" > ./encoded.b64
certutil -decode ./encoded.b64 ./password.txt > $null
certutil -decode ./encoded.b64 ./password.txt > $null
Get-Content ./password.txt
Remove-Item ./encoded.b64
Remove-Item ./password.txt