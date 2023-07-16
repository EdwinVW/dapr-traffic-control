#!/bin/bash

# specify 'consul' as the first argument to use consul for name resolution
if [ $1 == "consul" ]
then
    configFile=../dapr/config/consul-config.yaml
else
    configFile=../dapr/config/config.yaml
fi 

dapr run \
    --app-id finecollectionservice \
    --app-port 6001 \
    --dapr-http-port 3601 \
    --dapr-grpc-port 60001 \
    --config $configFile \
    --resources-path ../dapr/components dotnet run


