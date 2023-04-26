#!/bin/bash

PORT=6000

dapr run \
    --app-id trafficcontrolservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    -- dotnet run --urls http://localhost:$PORT
