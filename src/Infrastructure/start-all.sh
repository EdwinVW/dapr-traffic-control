#!/bin/bash

pushd mosquitto
./start-mosquitto.sh
popd

pushd rabbitmq
./start-rabbitmq.sh
popd

pushd maildev
./start-maildev.sh
popd