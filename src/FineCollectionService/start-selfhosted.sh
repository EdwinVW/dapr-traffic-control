#!/bin/bash

PORT=6001

dapr run \
    --app-id finecollectionservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    -- dotnet run --urls http://localhost:$PORT
