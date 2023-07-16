#!/bin/bash

# specify 'consul' as the first argument to use consul for name resolution
if [ $1 == "consul" ]
then
    configFile=../dapr/config/consul-config.yaml
else
    configFile=../dapr/config/config.yaml
fi

dapr run \
    --app-id trafficcontrolservice \
    --app-port 6000 \
    --dapr-http-port 3600 \
    --dapr-grpc-port 60000 \
    --config $configFile \
    --resources-path ../dapr/components dotnet run
