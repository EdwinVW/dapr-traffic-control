#!/bin/bash

docker run -d -p 5672:5672 -p 15672:15672 --name dtc-rabbitmq rabbitmq:3-management-alpine

docker run -d -p 4000:1080 -p 4025:1025 --name dtc-maildev maildev/maildev:2.0.5
