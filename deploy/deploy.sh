#!/bin/bash
# CheerDeck Deploy Script
# Builds and deploys all 3 apps to Azure App Service
# Prerequisites: Azure CLI logged in, resources created via azure-setup.sh

set -e

RESOURCE_GROUP="cheerdeck-prod"
CLUB_APP_NAME="cheerdeck-club"
COMPETITION_APP_NAME="cheerdeck-competition"
API_APP_NAME="cheerdeck-api"

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"

echo "Building API..."
dotnet publish "$ROOT_DIR/src/CheerDeck.Api/CheerDeck.Api.csproj" -c Release -o "$ROOT_DIR/publish/api"

echo "Building Club..."
dotnet publish "$ROOT_DIR/src/CheerDeck.Club.Web/CheerDeck.Club.Web.csproj" -c Release -o "$ROOT_DIR/publish/club"

echo "Building Competition..."
dotnet publish "$ROOT_DIR/src/CheerDeck.Competition.Web/CheerDeck.Competition.Web.csproj" -c Release -o "$ROOT_DIR/publish/competition"

echo "Deploying API..."
cd "$ROOT_DIR/publish/api"
zip -r "$ROOT_DIR/publish/api.zip" .
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --src "$ROOT_DIR/publish/api.zip"

echo "Deploying Club..."
cd "$ROOT_DIR/publish/club"
zip -r "$ROOT_DIR/publish/club.zip" .
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $CLUB_APP_NAME \
    --src "$ROOT_DIR/publish/club.zip"

echo "Deploying Competition..."
cd "$ROOT_DIR/publish/competition"
zip -r "$ROOT_DIR/publish/competition.zip" .
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $COMPETITION_APP_NAME \
    --src "$ROOT_DIR/publish/competition.zip"

echo ""
echo "=== DEPLOYED ==="
echo "API:         https://${API_APP_NAME}.azurewebsites.net"
echo "Club:        https://${CLUB_APP_NAME}.azurewebsites.net"
echo "Competition: https://${COMPETITION_APP_NAME}.azurewebsites.net"
