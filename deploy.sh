set -euo pipefail

LOCATION="${LOCATION:-brazilsouth}"
RG="rg-challenge-mottu"
ACR="acrchallengemottu"
IMAGE_REPO="appchallenge"
TAG="${TAG:-v1}"
IMAGE="${ACR}.azurecr.io/${IMAGE_REPO}:${TAG}"

DNS_LABEL="${DNS_LABEL:-app-challenge-mottu}"


ACR_USER="$(az acr credential show -n "$ACR" --query username -o tsv)"
ACR_PASS="$(az acr credential show -n "$ACR" --query 'passwords[0].value' -o tsv)"

DB_NAME="challengemottu_db"
DB_USER="postgres"
DB_PASS="${DB_PASS:-SenhaMuitoForte1234!}"
DB_PORT="5432"

APP_NAME="aci-app-challenge-mottu"
DB_NAME_ACI="aci-db-challenge-mottu"

echo ">>> Garantindo RG $RG ($LOCATION)"
az group create -n "$RG" -l "$LOCATION" -o table

echo RG=$RG ACR=$ACR APP=$APP_NAME DB=$DB_NAME_ACI


echo ">>> Criando ACI do Postgres (IP privado, sem exposição pública)"
DNS_LABEL_DB="${DNS_LABEL_DB:-db-challenge-mottu}"

az container create -g "$RG" -n "$DB_NAME_ACI" \
  --os-type Linux \
  --image postgres:17 \
  --cpu 1 --memory 1.5 \
  --ip-address Public \
  --dns-name-label "$DNS_LABEL_DB" \
  --ports $DB_PORT \
  --environment-variables \
    POSTGRES_USER="$DB_USER" \
    POSTGRES_PASSWORD="$DB_PASS" \
    POSTGRES_DB="$DB_NAME" \
  --restart-policy Always -o table

DB_FQDN="$(az container show -g "$RG" -n "$DB_NAME_ACI" --query ipAddress.fqdn -o tsv)"
echo "DB público: ${DB_FQDN}:${DB_PORT}"


echo ">>> Criando ACI do App com FQDN público"
az container create -g "$RG" -n "$APP_NAME" \
  --os-type Linux \
  --image "$IMAGE" \
  --cpu 1 --memory 1.5 \
  --registry-login-server "${ACR}.azurecr.io" \
  --registry-username "$ACR_USER" \
  --registry-password "$ACR_PASS" \
  --ip-address Public \
  --dns-name-label "$DNS_LABEL" \
  --ports 8080 \
  --environment-variables \
    ASPNETCORE_URLS="http://+:8080" \
    DOTNET_ENVIRONMENT="Production" \
    Jwt__Key="uqW8EXYt+3WOsDntgbG5Jt68rNTMmKZwpawNRcMIkSY=" \
    "ConnectionStrings__DefaultConnection=Host=${DB_FQDN};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}" \
  --restart-policy Always -o table

echo "Aguardando App ficar Running..."

FQDN="$(az container show -g "$RG" -n "$APP_NAME" --query ipAddress.fqdn -o tsv)"
APP_IP="$(az container show -g "$RG" -n "$APP_NAME" --query ipAddress.ip -o tsv)"

echo "App online!"
echo "FQDN: http://${FQDN}:8080"
echo "IP:   http://${APP_IP}:8080"
echo "DB:"
echo "${DB_FQDN}:${DB_PORT}"