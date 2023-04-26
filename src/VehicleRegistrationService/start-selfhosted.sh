#!/bin/bash

PORT=6003

dapr run \
    --app-id vehicleregistrationservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    --config ../../deployment/local/dapr/config/config.yaml \
    -- dotnet run --urls http://localhost:$PORT
