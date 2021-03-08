dapr run `
    --app-id trafficcontrolservice `
    --app-port 6000 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 60000 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run