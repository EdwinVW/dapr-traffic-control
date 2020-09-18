kubectl delete -n dapr-trafficcontrol `
    -f simulation.yaml `
    -f governmentservice.yaml `
    -f trafficcontrolservice.yaml `
    -f state-redis.yaml `
    -f pubsub-redis.yaml
