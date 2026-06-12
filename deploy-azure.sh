#!/bin/bash
set -e

# ============================================================
# CheerDeck Azure Deployment Script
# Run this in Azure Cloud Shell (https://shell.azure.com)
# ============================================================

# --- Configuration ---
RESOURCE_GROUP="cheerdeck-rg"
LOCATION="uksouth"
APP_PLAN="cheerdeck-plan"
SKU="B1"

API_APP="cheerdeck-api"
CLUB_APP="cheerdeck-club"
COMP_APP="cheerdeck-competition"

SQL_SERVER="cheerdeck-sql-$(openssl rand -hex 4)"
SQL_DB="CheerDeck"
SQL_ADMIN="cheerdeckadmin"
SQL_PASSWORD="CD-$(openssl rand -base64 16 | tr -d '/+=')!"

GITHUB_REPO="smithy14-lab/vigilant-meme"
GITHUB_BRANCH="main"

echo "============================================"
echo "  CheerDeck Azure Deployment"
echo "============================================"
echo ""
echo "Resource Group: $RESOURCE_GROUP"
echo "Location:       $LOCATION"
echo "SQL Server:     $SQL_SERVER"
echo ""

# --- 1. Resource Group ---
echo ">>> Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION --output none

# --- 2. App Service Plan ---
echo ">>> Creating App Service Plan ($SKU)..."
az appservice plan create \
  --name $APP_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku $SKU \
  --is-linux \
  --output none

# --- 3. Web Apps ---
echo ">>> Creating Web App: $API_APP..."
az webapp create \
  --name $API_APP \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_PLAN \
  --runtime "DOTNETCORE:9.0" \
  --output none

echo ">>> Creating Web App: $CLUB_APP..."
az webapp create \
  --name $CLUB_APP \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_PLAN \
  --runtime "DOTNETCORE:9.0" \
  --output none

echo ">>> Creating Web App: $COMP_APP..."
az webapp create \
  --name $COMP_APP \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_PLAN \
  --runtime "DOTNETCORE:9.0" \
  --output none

# --- 4. SQL Server + Database ---
echo ">>> Creating SQL Server: $SQL_SERVER..."
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password "$SQL_PASSWORD" \
  --output none

echo ">>> Allowing Azure services to access SQL..."
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

echo ">>> Creating database: $SQL_DB..."
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --service-objective Basic \
  --output none

CONNECTION_STRING="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Database=${SQL_DB};User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# --- 5. Configure App Settings ---
echo ">>> Configuring API app settings..."
az webapp config appsettings set \
  --name $API_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "UseInMemoryDatabase=false" \
    "Stripe__SecretKey=" \
    "Stripe__PublishableKey=" \
    "Stripe__WebhookSecret=" \
    "Brevo__ApiKey=" \
    "Brevo__FromEmail=noreply@cheerdeck.com" \
    "Brevo__FromName=CheerDeck" \
    "Cors__AllowedOrigins__0=https://${CLUB_APP}.azurewebsites.net" \
    "Cors__AllowedOrigins__1=https://${COMP_APP}.azurewebsites.net" \
  --output none

az webapp config connection-string set \
  --name $API_APP \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings "DefaultConnection=$CONNECTION_STRING" \
  --output none

echo ">>> Configuring Club app settings..."
az webapp config appsettings set \
  --name $CLUB_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "UseInMemoryDatabase=false" \
    "Stripe__SecretKey=" \
    "Stripe__PublishableKey=" \
    "Stripe__WebhookSecret=" \
    "Brevo__ApiKey=" \
    "Brevo__FromEmail=noreply@cheerdeck.com" \
    "Brevo__FromName=CheerDeck" \
  --output none

az webapp config connection-string set \
  --name $CLUB_APP \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings "DefaultConnection=$CONNECTION_STRING" \
  --output none

echo ">>> Configuring Competition app settings..."
az webapp config appsettings set \
  --name $COMP_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "UseInMemoryDatabase=false" \
    "Stripe__SecretKey=" \
    "Stripe__PublishableKey=" \
    "Stripe__WebhookSecret=" \
    "Brevo__ApiKey=" \
    "Brevo__FromEmail=noreply@cheerdeck.com" \
    "Brevo__FromName=CheerDeck" \
  --output none

az webapp config connection-string set \
  --name $COMP_APP \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings "DefaultConnection=$CONNECTION_STRING" \
  --output none

# --- 6. Set .NET environment ---
echo ">>> Setting production environment..."
for APP in $API_APP $CLUB_APP $COMP_APP; do
  az webapp config appsettings set \
    --name $APP \
    --resource-group $RESOURCE_GROUP \
    --settings "ASPNETCORE_ENVIRONMENT=Production" \
    --output none
done

# --- 7. Get Publish Profiles (for GitHub Actions) ---
echo ""
echo ">>> Downloading publish profiles..."
mkdir -p ~/cheerdeck-profiles

az webapp deployment list-publishing-profiles \
  --name $API_APP \
  --resource-group $RESOURCE_GROUP \
  --xml > ~/cheerdeck-profiles/api-publish-profile.xml

az webapp deployment list-publishing-profiles \
  --name $CLUB_APP \
  --resource-group $RESOURCE_GROUP \
  --xml > ~/cheerdeck-profiles/club-publish-profile.xml

az webapp deployment list-publishing-profiles \
  --name $COMP_APP \
  --resource-group $RESOURCE_GROUP \
  --xml > ~/cheerdeck-profiles/competition-publish-profile.xml

echo ""
echo "============================================"
echo "  DEPLOYMENT COMPLETE"
echo "============================================"
echo ""
echo "Web App URLs:"
echo "  API:         https://${API_APP}.azurewebsites.net"
echo "  Club:        https://${CLUB_APP}.azurewebsites.net"
echo "  Competition: https://${COMP_APP}.azurewebsites.net"
echo ""
echo "SQL Server:    ${SQL_SERVER}.database.windows.net"
echo "SQL Admin:     $SQL_ADMIN"
echo "SQL Password:  $SQL_PASSWORD"
echo ""
echo "SAVE THESE CREDENTIALS SOMEWHERE SAFE!"
echo ""
echo "============================================"
echo "  NEXT STEPS"
echo "============================================"
echo ""
echo "1. Add publish profiles as GitHub secrets:"
echo "   Go to: https://github.com/$GITHUB_REPO/settings/secrets/actions"
echo ""
echo "   Create these 3 secrets using the XML files in ~/cheerdeck-profiles/:"
echo "   - AZURE_API_PUBLISH_PROFILE         (from api-publish-profile.xml)"
echo "   - AZURE_CLUB_PUBLISH_PROFILE        (from club-publish-profile.xml)"
echo "   - AZURE_COMPETITION_PUBLISH_PROFILE  (from competition-publish-profile.xml)"
echo ""
echo "   To view a profile: cat ~/cheerdeck-profiles/api-publish-profile.xml"
echo ""
echo "2. Add your Stripe and Brevo keys in Azure Portal:"
echo "   Portal > App Services > [each app] > Configuration > Application settings"
echo "   Set: Stripe__SecretKey, Stripe__PublishableKey, Brevo__ApiKey"
echo ""
echo "3. Merge branch to main to trigger deployment:"
echo "   git checkout main && git merge claude/tender-meitner-l97oG && git push"
echo ""
echo "4. Set up Stripe webhook pointing to:"
echo "   https://${API_APP}.azurewebsites.net/api/webhooks/stripe"
echo ""
