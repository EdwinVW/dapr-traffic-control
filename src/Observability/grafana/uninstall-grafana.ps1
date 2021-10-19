# uninstall Grafana
helm uninstall grafana -n dapr-monitoring 

# uninstall Prometheus
helm uninstall dapr-prom -n dapr-monitoring 

# remove namespace
kubectl delete -f namespace.yaml