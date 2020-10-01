# kubectl create secret generic rdw-api-key --from-file ./rdw-api-key

kubectl apply -n dapr-trafficcontrol `
    -f secret.yaml `
    -f namespace.yaml `
    -f pubsub-redis.yaml `
    -f state-redis.yaml `
    -f trafficcontrolservice.yaml `
    -f governmentservice.yaml `
    -f simulation.yaml
