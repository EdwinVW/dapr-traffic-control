#!/bin/bash

configFile=../dapr/config/config.yaml

dapr run \
    --app-id finecollectionservice \
    --app-port 6001 \
    --dapr-http-port 3601 \
    --dapr-grpc-port 60001 \
    --config $configFile \
    --resources-path ../dapr/components dotnet run


