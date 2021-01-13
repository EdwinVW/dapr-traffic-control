kubectl delete `
    -f simulation.yaml `
    -f governmentservice.yaml `
    -f trafficcontrolservice.yaml `
    -f state-redis.yaml `
    -f pubsub-redis.yaml `
    -f secret.yaml `
    -f zipkin.yaml `
    -f dapr-config.yaml
