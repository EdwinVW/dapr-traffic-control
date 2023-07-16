#!/bin/bash

pushd mosquitto
./stop-mosquitto.sh
popd

pushd rabbitmq
./stop-rabbitmq.sh
popd

pushd maildev
./stop-maildev.sh
popd

# specify 'consul' as the first argument if you've used consul for name resolution
if [ $1 == "consul" ]
then
    pushd consul
    ./stop-consul.sh
    popd
fi
