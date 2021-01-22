Start-Process "http://localhost:8080"
kubectl port-forward svc/grafana 8080:80 -n dapr-monitoring