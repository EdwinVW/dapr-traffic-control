dapr run `
    --app-id governmentservice `
    --app-port 6000 `
    --dapr-http-port 3500 `
    --dapr-grpc-port 50002 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run