#!/bin/bash

RESOURCE_GROUP=dapr-traffic-control

az containerapp update --name trafficcontrolservice --resource-group $RESOURCE_GROUP --image ghcr.io/ondfisk/traffic-control-service:latest
az containerapp update --name finecollectionservice --resource-group $RESOURCE_GROUP --image ghcr.io/ondfisk/fine-collection-service:latest
az containerapp update --name vehicleregistrationservice --resource-group $RESOURCE_GROUP --image ghcr.io/ondfisk/vehicle-registration-service:latest
