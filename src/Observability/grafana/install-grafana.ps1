kubectl apply -f namespace.yaml

helm repo add grafana https://grafana.github.io/helm-charts
helm repo update
helm install grafana grafana/grafana -n dapr-monitoring 

#--set persistence.enabled=false

# display Grafana admin password
& ./get-grafana-password.ps1