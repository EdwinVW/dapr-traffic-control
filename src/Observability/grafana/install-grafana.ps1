kubectl apply -f namespace.yaml

# Get/update helm charts
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

# Install Prometheus
helm install dapr-prom prometheus-community/prometheus -n dapr-monitoring --set nodeExporter.hostRootfs=false

# Install Grafana
helm install grafana grafana/grafana -n dapr-monitoring 

# Display Grafana admin password
& ./get-grafana-password.ps1