#!/bin/bash

docker build -t ghcr.io/ondfisk/fine-collection-service:latest ./FineCollectionService
docker push ghcr.io/ondfisk/fine-collection-service:latest

docker build -t ghcr.io/ondfisk/traffic-control-service:latest ./TrafficControlService
docker push ghcr.io/ondfisk/traffic-control-service:latest

docker build -t ghcr.io/ondfisk/vehicle-registration-service:latest ./VehicleRegistrationService
docker push ghcr.io/ondfisk/vehicle-registration-service:latest
