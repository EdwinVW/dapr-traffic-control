dapr run `
    --app-id trafficcontrolservice `
    --app-port 5000 `
    --dapr-http-port 3500 `
    --dapr-grpc-port 50000 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run