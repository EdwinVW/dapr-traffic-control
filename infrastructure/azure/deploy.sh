#!/bin/bash

LOCATION=swedencentral
RESOURCE_GROUP=dapr-traffic-control
TEMPLATE_FILE_PATH="./main.bicep"
PARAMETERS_FILE_PATH="./main.parameters.json"
TABLE_NAME=vehicles
SERVICE_BUS_QUEUE_NAME=speedingviolations

# Parse the parameters file and set the values to variables
PARAMETERS=$(jq -r '.parameters' $PARAMETERS_FILE_PATH)

for param in $(echo "${PARAMETERS}" | jq -r 'keys[]'); do
  key=$(echo "$param" | sed 's/\([a-z]\)\([A-Z]\)/\1_\2/g' | tr '[:lower:]' '[:upper:]')
  value=$(echo "${PARAMETERS}" | jq -r ".$param.value")
  eval "$key='$value'"
done

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy resources
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file $TEMPLATE_FILE_PATH \
  --parameters @$PARAMETERS_FILE_PATH

# Create service bus queue
az servicebus queue create \
  --name $SERVICE_BUS_QUEUE_NAME \
  --namespace-name $SERVICE_BUS_NAMESPACE_NAME \
  --resource-group $RESOURCE_GROUP

# Get service bus queue id
SERVICE_BUS_QUEUE_ID=$(az servicebus queue show --name $SERVICE_BUS_QUEUE_NAME --namespace-name $SERVICE_BUS_NAMESPACE_NAME --resource-group $RESOURCE_GROUP --query id -o tsv)

# Create storage table
az storage table create \
  --name $TABLE_NAME \
  --account-name $STORAGE_ACCOUNT_NAME \
  --auth-mode login

# Get storage table id
TABLE_ID=$(az storage table show --name $TABLE_NAME --account-name $STORAGE_ACCOUNT_NAME --query id -o tsv)

# Configure secretstore component
az containerapp env dapr-component set \
    --name $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --dapr-component-name secretstore \
    --yaml ./dapr/components/secretstore.yaml

# Configure statestore component
az containerapp env dapr-component set \
    --name $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --dapr-component-name statestore \
    --yaml ./dapr/components/statestore.yaml

# Configure pubsub component
az containerapp env dapr-component set \
    --name $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --dapr-component-name pubsub \
    --yaml ./dapr/components/pubsub.yaml

# Configure sendmail component
az containerapp env dapr-component set \
    --name $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --dapr-component-name sendmail \
    --yaml ./dapr/components/sendmail.yaml

# Get key vault id
KEY_VAULT_ID=$(az keyvault show --name $KEY_VAULT_NAME --query id -o tsv)

# Create vehicle-registration-service container app
az containerapp create \
    --name vehicleregistrationservice \
    --environment $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --image ghcr.io/ondfisk/vehicle-registration-service:latest \
    --enable-dapr \
    --dapr-enable-api-logging \
    --dapr-app-id vehicleregistrationservice \
    --dapr-app-protocol http \
    --cpu '0.25' \
    --memory '0.5Gi' \
    --system-assigned \
    --ingress external \
    --target-port 6002 \
    --min-replicas 1 \
    --max-replicas 1

# Create fine-collection-service container app
az containerapp create \
    --name finecollectionservice \
    --environment $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --image ghcr.io/ondfisk/fine-collection-service:latest \
    --enable-dapr \
    --dapr-enable-api-logging \
    --dapr-app-id finecollectionservice \
    --dapr-app-protocol http \
    --cpu '0.25' \
    --memory '0.5Gi' \
    --system-assigned \
    --ingress external \
    --target-port 6002 \
    --min-replicas 1 \
    --max-replicas 1 \
    --env-vars "FINE_CALCULATOR_LICENSE_SECRET_NAME=fine-calculator-license-key"

# Get container app principal id
FINE_COLLECTION_SERVICE_PRINCIPAL_ID=$(az containerapp show --name finecollectionservice --resource-group $RESOURCE_GROUP --query identity.principalId -o tsv)

# Assign secret reader role to Fine Collection Service
az role assignment create \
  --assignee $FINE_COLLECTION_SERVICE_PRINCIPAL_ID \
  --role "Key Vault Secrets User" \
  --scope $KEY_VAULT_ID

# Assign service bus data receiver to Fine Collection Service
az role assignment create \
  --assignee $TRAFFIC_CONTROL_SERVICE_PRINCIPAL_ID \
  --role "Azure Service Bus Data Receiver" \
  --scope $SERVICE_BUS_QUEUE_ID

# Create traffic-control-service container app

# TODO - Add traffic-control-service container app

# Get container app principal id
TRAFFIC_CONTROL_SERVICE_PRINCIPAL_ID=$(az containerapp show --name trafficcontrolservice --resource-group $RESOURCE_GROUP --query identity.principalId -o tsv)

# Assign table writer role to Traffic Control Service
az role assignment create \
  --assignee $FINE_COLLECTION_SERVICE_PRINCIPAL_ID \
  --role "Storage Table Data Contributor" \
  --scope $TABLE_ID

# Assign service bus data sender to Traffic Control Service
az role assignment create \
  --assignee $TRAFFIC_CONTROL_SERVICE_PRINCIPAL_ID \
  --role "Azure Service Bus Data Sender" \
  --scope $SERVICE_BUS_QUEUE_ID

# Create maildev container instance
az container create \
    --name maildev \
    --resource-group $RESOURCE_GROUP \
    --image maildev/maildev:latest \
    --dns-name-label maildev \
    --ports 1080 1025

# Create fine-collection-service container app
az containerapp create \
    --name themailservice \
    --environment $CONTAINER_APPS_ENVIRONMENT_NAME \
    --resource-group $RESOURCE_GROUP \
    --image ghcr.io/ondfisk/the-mail-service:latest \
    --enable-dapr \
    --dapr-enable-api-logging \
    --dapr-app-id themailservice \
    --dapr-app-protocol http \
    --cpu '0.25' \
    --memory '0.5Gi' \
    --system-assigned \
    --ingress external \
    --target-port 6014 \
    --min-replicas 1 \
    --max-replicas 1

# Register cameras 0-3 on IoT hub
# bash for loop 0-3
for i in {0..3}; do
  az iot hub device-identity create \
    --hub-name $IOT_HUB_NAME \
    --device-id camerasim$i
done
