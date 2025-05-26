# === Variáveis de configuração ===
RG="rg-mottuguard"                          # nome do Resource Group
LOCATION="brazilsouth"                     # região Azure
VM_NAME="vm-mottuguard"                    # nome da VM
ADMIN_USER="azureuser"                     # usuário administrador da VM
SSH_KEY_PATH="$HOME/.ssh/id_rsa.pub"       # caminho para sua chave SSH pública
IMAGE_NAME="seu-usuario/mottuguard:latest" # imagem no Docker Hub
CONTAINER_NAME="mottuguard-backend"        # nome do container
HOST_PORT=5000                             # porta exposta no host
CONTAINER_PORT=80                          # porta que o container irá escutar

# 1) Criar Resource Group
az group create \
  --name "$RG" \
  --location "$LOCATION"
echo "➜ Resource Group '$RG' criado em $LOCATION"

# 2) Criar a VM Ubuntu e provisionar IP público
az vm create \
  --resource-group "$RG" \
  --name "$VM_NAME" \
  --image UbuntuLTS \
  --admin-username "$ADMIN_USER" \
  --ssh-key-value "$SSH_KEY_PATH" \
  --size Standard_B1s \
  --public-ip-sku Standard \
  --output none
echo "➜ VM '$VM_NAME' criada com usuário '$ADMIN_USER'"

# 3) Abrir a porta no NSG para acesso externo
az vm open-port \
  --resource-group "$RG" \
  --name "$VM_NAME" \
  --port "$HOST_PORT"
echo "➜ Porta $HOST_PORT aberta no Security Group da VM"

# 4) Instalar Docker e executar o container via Run Command
az vm run-command invoke \
  --resource-group "$RG" \
  --name "$VM_NAME" \
  --command-id RunShellScript \
  --scripts \
"sudo apt-get update
sudo apt-get install -y docker.io
sudo systemctl enable docker
sudo docker pull $IMAGE_NAME
sudo docker rm -f $CONTAINER_NAME || true
sudo docker run -d \
    --name $CONTAINER_NAME \
    --restart unless-stopped \
    -p $HOST_PORT:$CONTAINER_PORT \
    $IMAGE_NAME"
echo "➜ Docker instalado e container '$CONTAINER_NAME' rodando na porta $HOST_PORT"

# 5) Exibir IP público para acesso
PUBLIC_IP=$(az vm show -d \
  --resource-group "$RG" \
  --name "$VM_NAME" \
  --query publicIps -o tsv)

echo
echo "Deploy concluído! Acesse sua API em: http://$PUBLIC_IP:$HOST_PORT"