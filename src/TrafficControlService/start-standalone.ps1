dapr run `
    --app-id trafficcontrolservice `
    --app-port 5000 `
    --dapr-grpc-port 50001 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    --app-max-concurrency 1 `
    dotnet run