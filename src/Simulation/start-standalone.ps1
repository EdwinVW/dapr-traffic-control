dapr run `
    --app-id simulation `
    --dapr-grpc-port 50003 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run