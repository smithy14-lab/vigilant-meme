#!/bin/bash
# CheerDeck Azure Deployment Setup
# Run this once to create all Azure resources
# Prerequisites: Azure CLI installed and logged in (az login)

set -e

# Configuration - change these as needed
RESOURCE_GROUP="cheerdeck-prod"
LOCATION="uksouth"
SQL_SERVER_NAME="cheerdeck-sql"
SQL_DB_NAME="cheerdeck-db"
SQL_ADMIN_USER="cheerdeckadmin"
SQL_ADMIN_PASS="" # Set this before running!
APP_SERVICE_PLAN="cheerdeck-plan"
CLUB_APP_NAME="cheerdeck-club"
COMPETITION_APP_NAME="cheerdeck-competition"
API_APP_NAME="cheerdeck-api"

if [ -z "$SQL_ADMIN_PASS" ]; then
    echo "ERROR: Set SQL_ADMIN_PASS before running this script"
    exit 1
fi

echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "Creating SQL Server..."
az sql server create \
    --name $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_USER \
    --admin-password $SQL_ADMIN_PASS

echo "Creating SQL Database..."
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name $SQL_DB_NAME \
    --service-objective S0

echo "Allowing Azure services to access SQL..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

echo "Creating App Service Plan (B1 tier)..."
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --sku B1 \
    --is-linux

CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASS};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

echo "Creating API app..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $API_APP_NAME \
    --runtime "DOTNETCORE:9.0"

az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --settings \
    "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    "UseInMemoryDatabase=false" \
    "ASPNETCORE_URLS=http://+:8080"

echo "Creating Club app..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $CLUB_APP_NAME \
    --runtime "DOTNETCORE:9.0"

az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $CLUB_APP_NAME \
    --settings \
    "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    "UseInMemoryDatabase=false" \
    "ASPNETCORE_URLS=http://+:8080"

echo "Creating Competition app..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $COMPETITION_APP_NAME \
    --runtime "DOTNETCORE:9.0"

az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $COMPETITION_APP_NAME \
    --settings \
    "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    "UseInMemoryDatabase=false" \
    "ASPNETCORE_URLS=http://+:8080"

echo ""
echo "=== DONE ==="
echo "API URL:         https://${API_APP_NAME}.azurewebsites.net"
echo "Club URL:        https://${CLUB_APP_NAME}.azurewebsites.net"
echo "Competition URL: https://${COMPETITION_APP_NAME}.azurewebsites.net"
echo ""
echo "To deploy, run: deploy/deploy.sh"
echo "To add Stripe/SendGrid keys, run:"
echo "  az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $CLUB_APP_NAME --settings Stripe__SecretKey=sk_live_xxx SendGrid__ApiKey=SG.xxx"
