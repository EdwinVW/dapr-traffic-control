#!/bin/bash

configFile=../dapr/config/config.yaml

dapr run \
    --app-id vehicleregistrationservice \
    --app-port 6002 \
    --dapr-http-port 3602 \
    --dapr-grpc-port 60002 \
    --config $configFile \
    --resources-path ../dapr/components dotnet run
