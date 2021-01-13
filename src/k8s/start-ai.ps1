kubectl apply `
    -f namespace.yaml `
    -f ./open-telemetry/dapr-otel-config.yaml `
    -f ./open-telemetry/otel-collector.yaml `
    -f secret.yaml `
    -f pubsub-redis.yaml `
    -f state-redis.yaml `
    -f trafficcontrolservice.yaml `
    -f governmentservice.yaml `
    -f simulation.yaml
