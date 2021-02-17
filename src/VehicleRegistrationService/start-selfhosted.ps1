dapr run `
    --app-id vehicleregistrationservice `
    --app-port 5002 `
    --dapr-http-port 3502 `
    --dapr-grpc-port 50002 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run