#!/bin/bash

pushd consul
./start-consul.sh
popd

pushd mosquitto
./start-mosquitto.sh
popd

pushd rabbitmq
./start-rabbitmq.sh
popd

pushd maildev
./start-maildev.sh
popd