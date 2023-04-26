#!/bin/bash

PORT=6002

dapr run \
    --app-id finecollectionservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    --config ../../deployment/local/dapr/config/config.yaml \
    -- dotnet run --urls http://localhost:$PORT
