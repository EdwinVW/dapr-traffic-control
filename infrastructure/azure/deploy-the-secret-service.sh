#!/bin/bash

LOCATION=swedencentral
RESOURCE_GROUP=dapr-traffic-control
CONTAINER_APPS_ENVIRONMENT=trafficcontrolenvironment
KEY_VAULT=trafficcontrolvault

az group create --name $RESOURCE_GROUP --location $LOCATION

# Configure secretstore component
az containerapp env dapr-component set \
    --name $CONTAINER_APPS_ENVIRONMENT \
    --resource-group $RESOURCE_GROUP \
    --dapr-component-name secretstore \
    --yaml ./dapr/components/secretstore.yaml

# Create the-secret-service container app
az containerapp create \
    --name thesecretservice2 \
    --environment $CONTAINER_APPS_ENVIRONMENT \
    --resource-group $RESOURCE_GROUP \
    --image ghcr.io/ondfisk/the-secret-service:latest \
    --enable-dapr \
    --dapr-enable-api-logging \
    --dapr-app-id thesecretservice \
    --dapr-app-protocol http \
    --cpu '0.25' \
    --memory '0.5Gi' \
    --system-assigned \
    --ingress external \
    --target-port 6013 \
    --min-replicas 1 \
    --max-replicas 1

# Get container app principal id
THE_SECRET_SERVICE_PRINCIPAL_ID=$(az containerapp show --name thesecretservice --resource-group $RESOURCE_GROUP --query identity.principalId -o tsv)

# Get key vault id
KEYVAULT_ID=$(az keyvault show --name $KEY_VAULT --query id -o tsv)

# Assign secret reader role to container app principal
az role assignment create \
  --assignee $THE_SECRET_SERVICE_PRINCIPAL_ID \
  --role "Key Vault Secrets User" \
  --scope $KEYVAULT_ID
