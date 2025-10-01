set -euo pipefail

LOCATION="${LOCATION:-brazilsouth}"
RG="rg-challenge-mottu"
ACR="acrchallengemottu"
IMAGE_REPO="appchallenge"
TAG="${TAG:-v1}"
IMAGE="${ACR}.azurecr.io/${IMAGE_REPO}:${TAG}"

echo ">>> Login na Azure"
az account show >/dev/null 2>&1 || az login -o table

echo ">>> Criando Resource Group: $RG ($LOCATION)"
az group create -n "$RG" -l "$LOCATION" -o table

echo ">>> Criando ACR: $ACR"
az acr create -g "$RG" -n "$ACR" --sku Basic -o table || true

az acr update -n "$ACR" --admin-enabled true -o table

az acr build \
  --registry "$ACR" \
  --image "${IMAGE_REPO}:${TAG}" \
  --file Dockerfile \
  .

echo "Done! Imagem publicada no ACR:"
echo "  $IMAGE"