kubectl delete `
    -f simulation.yaml `
    -f governmentservice.yaml `
    -f trafficcontrolservice.yaml `
    -f state-redis.yaml `
    -f pubsub-redis.yaml `
    -f secret.yaml `
    -f ./open-telemetry/otel-collector.yaml `
    -f ./open-telemetry/dapr-otel-config.yaml
