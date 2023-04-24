#!/bin/bash

PORT=6001

dapr run \
    --app-id vehicleregistrationservice \
    --app-port $PORT \
    --config ../dapr/config/config.yaml \
    --resources-path ../dapr/components \
    -- dotnet run --urls http://localhost:$PORT
