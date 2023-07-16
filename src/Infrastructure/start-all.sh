#!/bin/bash

# specify 'consul' as the first argument to use consul for name resolution
if [ $1 == "consul" ]
then
    pushd consul
    ./start-consul.sh
    popd
fi 

pushd mosquitto
./start-mosquitto.sh
popd

pushd rabbitmq
./start-rabbitmq.sh
popd

pushd maildev
./start-maildev.sh
popd