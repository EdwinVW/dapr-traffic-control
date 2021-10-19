# display Grafana admin password
 $password = kubectl get secret --namespace dapr-monitoring grafana -o jsonpath="{.data.admin-password}"
 [Text.Encoding]::Utf8.GetString([Convert]::FromBase64String($password))