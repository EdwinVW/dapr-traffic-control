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