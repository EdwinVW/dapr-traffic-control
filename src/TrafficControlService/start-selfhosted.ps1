dapr run `
    --app-id trafficcontrolservice `
    --app-port 6000 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 3700 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run