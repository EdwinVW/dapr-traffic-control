dapr run `
    --app-id finecollectionservice `
    --app-port 5001 `
    --dapr-http-port 3501 `
    --dapr-grpc-port 50001 `
    --config ../dapr/config/config.yaml `
    --components-path ../dapr/components `
    dotnet run