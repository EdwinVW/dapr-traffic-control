dapr run `
    --app-id vehicleregistrationservice `
    --app-port 6002 `
    --dapr-http-port 3602 `
    --dapr-grpc-port 60002 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run