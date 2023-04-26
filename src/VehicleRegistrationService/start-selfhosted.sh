#!/bin/bash

PORT=6002

dapr run \
    --app-id vehicleregistrationservice \
    --app-port $PORT \
    --resources-path ../../deployment/local/dapr/components \
    -- dotnet run --urls http://localhost:$PORT
