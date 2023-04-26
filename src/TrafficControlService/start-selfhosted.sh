#!/bin/bash

PORT=6001

dapr run \
    --app-id trafficcontrolservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    --config ../../deployment/local/dapr/config/config.yaml \
    -- dotnet run --urls http://localhost:$PORT
