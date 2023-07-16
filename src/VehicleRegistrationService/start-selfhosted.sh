#!/bin/bash

# specify 'consul' as the first argument to use consul for name resolution
if [ $1 == "consul" ]
then
    configFile=../dapr/config/consul-config.yaml
else
    configFile=../dapr/config/config.yaml
fi 

dapr run \
    --app-id vehicleregistrationservice \
    --app-port 6002 \
    --dapr-http-port 3602 \
    --dapr-grpc-port 60002 \
    --config $configFile \
    --resources-path ../dapr/components dotnet run
