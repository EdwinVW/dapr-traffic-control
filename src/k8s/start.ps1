kubectl apply -n dapr-trafficcontrol `
    -f namespace.yaml `
    -f pubsub-redis.yaml `
    -f state-redis.yaml `
    -f trafficcontrolservice.yaml `
    -f governmentservice.yaml `
    -f simulation.yaml
