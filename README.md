# CP4-DEVOPS — ClyvoVet

Checkpoint 2º Semestre — Containers em Nuvem (ACR/ACI)

Deploy de uma API .NET + Banco de Dados Oracle como containers na Azure, utilizando **Docker** (build/run separados, sem Compose) para build e testes locais, e **Azure Container Registry (ACR)** + **Azure Container Instances (ACI)** para o registro e execução em nuvem — seguindo o passo a passo apresentado na Aula 12 (ACR/ACI).

## Pré-requisitos

- Docker instalado e em execução na sua máquina
- Azure CLI instalada (`az`) na sua máquina
- Conta ativa no Microsoft Azure
- Git

## Dados do projeto

| Item | Valor |
|---|---|
| RM | `564662` |
| Localização | `canadacentral` |
| Resource Group | `rg-clyvovet-564662` |
| ACR | `clyvovet564662` |
| Storage Account | `stclyvovet564662` |
| Key Vault | `kvclyvovet564662` |
| Container (Banco) | `oracledb-564662` |
| Container (API) | `clyvovetapi-564662` |

## Dockerfiles do projeto

### `Dockerfile.oracle` (banco de dados)

```dockerfile
FROM gvenzl/oracle-xe:latest

# Senha do usuário SYS/SYSTEM do Oracle XE
ENV ORACLE_PASSWORD=020207

# Scripts .sql colocados nesta pasta são executados automaticamente
# na primeira inicialização do banco (ex: DDL das tabelas do projeto)
COPY ./init-scripts/ /container-entrypoint-initdb.d/

EXPOSE 1521

VOLUME ["/opt/oracle/oradata"]
```

### `Dockerfile.api` (API .NET)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies to optimize Docker layer caching
COPY ["ClyvoVetApi.csproj", "./"]
RUN dotnet restore "ClyvoVetApi.csproj"

# Copy the remaining files and build
COPY . .
RUN dotnet build "ClyvoVetApi.csproj" -c Release -o /app/build
RUN dotnet publish "ClyvoVetApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the secure aspnet image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose non-privileged HTTP port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Enforce secure non-root user execution (DevOps Compliance)
USER app

ENTRYPOINT ["dotnet", "ClyvoVetApi.dll"]
```

> Ajuste a versão do SDK/runtime (`9.0`) para a versão do .NET usada no projeto, e o nome da DLL no `ENTRYPOINT` para o nome real do assembly gerado (geralmente `<NomeDoProjeto>.dll`).

---

## Parte 1 — Login na Azure e criação do Resource Group

### 1. Login na Azure

```bash
az login
az account show
```

Se necessário, selecione a assinatura correta:

```bash
az account list -o table
az account set --subscription "<nome ou id>"
```

### 2. Criar o Resource Group

```bash
az group create --name rg-clyvovet-564662 --location canadacentral
```

---

## Parte 2 — Build e testes locais

### 3. Clonar o repositório

```bash
git clone https://github.com/AndreBellandi/CP4-DEVOPS-.git
cd CP4-DEVOPS-/ClyvoVetApi-main
```

### 4. Criar uma rede Docker local

```bash
docker network create clyvovet-net
```

### 5. Build das imagens localmente

```bash
docker build -f Dockerfile.oracle -t oracledb-564662 .
docker build -f Dockerfile.api -t clyvovetapi-564662 .
```

---

## Parte 3 — Registrar as imagens no ACR

### 10. Registrar o provider e criar o Azure Container Registry (ACR)

```bash
az provider register --namespace Microsoft.ContainerRegistry

az acr create \
    --resource-group rg-clyvovet-564662 \
    --name clyvovet564662 \
    --sku Standard \
    --location canadacentral \
    --public-network-enabled true \
    --admin-enabled true
```

### 11. Obter credenciais do ACR

```bash
LOGIN_SERVER=$(az acr show --name clyvovet564662 \
                           --resource-group rg-clyvovet-564662 \
                           --query loginServer --output tsv)
echo ""
echo "Login Server: $LOGIN_SERVER"
echo ""

ADMIN_USERNAME=$(az acr credential show --name clyvovet564662 \
                                        --resource-group rg-clyvovet-564662 \
                                        --query username --output tsv) && \
ADMIN_PASSWORD=$(az acr credential show --name clyvovet564662 \
                                        --resource-group rg-clyvovet-564662 \
                                 --query passwords[0].value --output tsv) && \
echo "Username: $ADMIN_USERNAME" && echo "Password: $ADMIN_PASSWORD"
```

### 12. Login no ACR

```bash
az acr login --name clyvovet564662
```

ou

```bash
docker login $LOGIN_SERVER \
  -u $ADMIN_USERNAME \
  -p $ADMIN_PASSWORD
```

> Como visto na Aula 12, também é possível logar sem o Docker instalado, usando `az acr login --name clyvovet564662 --expose-token` e repassando o token via `docker login ... --password-stdin`.

### 13. Tag e push das imagens para o ACR

```bash
docker image ls

docker tag oracledb-564662 $LOGIN_SERVER/oracledb-564662:v1
docker push $LOGIN_SERVER/oracledb-564662:v1

docker tag clyvovetapi-564662 $LOGIN_SERVER/clyvovetapi-564662:v1
docker push $LOGIN_SERVER/clyvovetapi-564662:v1
```

### 14. Conferir imagens registradas no ACR

```bash
az acr repository list --name clyvovet564662 --output table
```

Comandos úteis adicionais:

```bash
# Listar as tags (imagens) de um repositório
az acr repository show-tags --name clyvovet564662 --repository clyvovetapi-564662

# Mostrar manifesto
az acr manifest list-metadata --registry clyvovet564662 --name clyvovetapi-564662

# Limpar imagens antigas do ACR
az acr purge --name clyvovet564662 --filter 'clyvovetapi-564662:.*' --ago 7d --untagged

# Detalhes de um repositório
az acr repository show --name clyvovet564662 --repository oracledb-564662

# Habilitar usuário administrador
az acr update --name clyvovet564662 --admin-enabled true
```

### 15. (Opcional) Remover imagens locais após o push

```bash
docker rmi $LOGIN_SERVER/oracledb-564662:v1
docker rmi $LOGIN_SERVER/clyvovetapi-564662:v1
```

---

## Parte 4 — Criar os recursos de suporte em nuvem


### 16. Script `01_store-account.sh` — Conta de Armazenamento

```bash
#!/bin/bash
set -e

RESOURCE_GROUP="rg-clyvovet-564662"
LOCATION="canadacentral"
STORAGE_ACCOUNT="stclyvovet564662"
FILE_SHARE="oracle-data"

az storage account create \
  --resource-group $RESOURCE_GROUP \
  --name $STORAGE_ACCOUNT \
  --location $LOCATION \
  --sku Standard_LRS

STORAGE_KEY=$(az storage account keys list \
  --resource-group $RESOURCE_GROUP \
  --account-name $STORAGE_ACCOUNT \
  --query "[0].value" --output tsv)

az storage share create \
  --name $FILE_SHARE \
  --account-name $STORAGE_ACCOUNT \
  --account-key $STORAGE_KEY

echo ""
echo "Storage Account: $STORAGE_ACCOUNT"
echo "File Share: $FILE_SHARE"
echo "Storage Key: $STORAGE_KEY"
echo ""
```

```bash
chmod +x 01_store-account.sh
./01_store-account.sh > 01_store-account.log
```

### 17. Script `02_key-vault.sh` — Key Vault

```bash
#!/bin/bash
set -e

RESOURCE_GROUP="rg-clyvovet-564662"
LOCATION="canadacentral"
KEY_VAULT="kvclyvovet564662"

az keyvault create \
  --resource-group $RESOURCE_GROUP \
  --name $KEY_VAULT \
  --location $LOCATION

az keyvault secret set --vault-name $KEY_VAULT --name "oracle-password" --value "020207"
az keyvault secret set --vault-name $KEY_VAULT --name "storage-account-name" --value "stclyvovet564662"
az keyvault secret set --vault-name $KEY_VAULT --name "storage-account-key" --value "$STORAGE_KEY"
az keyvault secret set --vault-name $KEY_VAULT --name "acr-username" --value "$ADMIN_USERNAME"
az keyvault secret set --vault-name $KEY_VAULT --name "acr-password" --value "$ADMIN_PASSWORD"

echo ""
echo "Key Vault: $KEY_VAULT criado e segredos armazenados"
echo ""
```

```bash
chmod +x 02_key-vault.sh
./02_key-vault.sh > 02_key-vault.log
```

> Verifique a Conta de Armazenamento e o Key Vault criados no Portal do Azure.

---

## Parte 5 — Deploy em Azure Container Instances (ACI)

### 18. Script `03_aci-oracledb.sh` — ACI do banco Oracle

```bash
#!/bin/bash
set -e

RESOURCE_GROUP="rg-clyvovet-564662"
LOGIN_SERVER="clyvovet564662.azurecr.io"

az container create \
  --resource-group $RESOURCE_GROUP \
  --name oracledb-564662 \
  --image $LOGIN_SERVER/oracledb-564662:v1 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ADMIN_USERNAME \
  --registry-password $ADMIN_PASSWORD \
  --ports 1521 \
  --cpu 2 \
  --memory 4 \
  --environment-variables ORACLE_PASSWORD=020207 \
  --azure-file-volume-account-name $STORAGE_ACCOUNT \
  --azure-file-volume-account-key $STORAGE_KEY \
  --azure-file-volume-share-name oracle-data \
  --azure-file-volume-mount-path /opt/oracle/oradata

echo "Aguardando o container do Oracle iniciar..."
az container logs --resource-group $RESOURCE_GROUP --name oracledb-564662

FQDN_DB=$(az container show --resource-group $RESOURCE_GROUP --name oracledb-564662 \
  --query ipAddress.ip --output tsv)

echo ""
echo "IP do oracledb-564662: $FQDN_DB"
echo ""
```

```bash
chmod +x 03_aci-oracledb.sh
./03_aci-oracledb.sh > 03_aci-oracledb.log
```

> O volume `--azure-file-volume-*` persiste os dados do banco em uma Conta de Armazenamento, conforme exigido no checkpoint.

### 19. Script `04_aci-clyvovetapi.sh` — ACI da API .NET

```bash
#!/bin/bash
set -e

RESOURCE_GROUP="rg-clyvovet-564662"
LOGIN_SERVER="clyvovet564662.azurecr.io"

az container create \
  --resource-group $RESOURCE_GROUP \
  --name clyvovetapi-564662 \
  --image $LOGIN_SERVER/clyvovetapi-564662:v1 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ADMIN_USERNAME \
  --registry-password $ADMIN_PASSWORD \
  --ports 8080 \
  --dns-name-label clyvovetapi-564662 \
  --environment-variables ASPNETCORE_URLS=http://+:8080 \
  --secure-environment-variables ORACLE_CONNECTION_STRING="Data Source=$FQDN_DB:1521/XEPDB1;User Id=system;Password=020207;"

echo "Aguardando o container da API iniciar..."
az container logs --resource-group $RESOURCE_GROUP --name clyvovetapi-564662

FQDN_API=$(az container show --resource-group $RESOURCE_GROUP --name clyvovetapi-564662 \
  --query ipAddress.fqdn --output tsv)

echo ""
echo "FQDN da clyvovetapi-564662: $FQDN_API"
echo ""

curl -X GET http://$FQDN_API:8080/api/transacoes
```

```bash
chmod +x 04_aci-clyvovetapi.sh
./04_aci-clyvovetapi.sh > 04_aci-clyvovetapi.log
```

> Verifique os dois ACIs criados no Portal do Azure junto com o Resource Group `rg-clyvovet-564662`.

---

## Parte 6 — Testes de CRUD em nuvem

**POST**

```bash
curl -X POST http://$FQDN_API:8080/api/transacoes \
  -H "Content-Type: application/json" \
  -d '{
    "descricao": "Compra no supermercado",
    "valor": 150.75
  }'
```

**GET**

```bash
curl -X GET http://$FQDN_API:8080/api/transacoes
```

**PUT**

```bash
curl -X PUT http://$FQDN_API:8080/api/transacoes/6 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 6,
    "descricao": "Compra no supermercado - ALTERADO",
    "valor": 150.76,
    "dataTransacao": "2024-06-18T00:00:00"
  }'
```

**DELETE**

```bash
curl -X DELETE http://$FQDN_API:8080/api/transacoes/6
```

### 20. Conferir a persistência direto no Oracle

```bash
az container exec --resource-group rg-clyvovet-564662 --name oracledb-564662 --exec-command "/bin/bash"

sqlplus system/020207@localhost:1521/XEPDB1

SELECT * FROM transacoes;
```

---

## Comandos úteis de operação/troubleshooting

```bash
# Logs do container
az container logs --resource-group rg-clyvovet-564662 --name clyvovetapi-564662

# Logs com streaming
az container logs --resource-group rg-clyvovet-564662 --name clyvovetapi-564662 --follow

# Sessão interativa
az container exec --resource-group rg-clyvovet-564662 --name clyvovetapi-564662 --exec-command "/bin/bash"

# Verificar processos
az container exec --resource-group rg-clyvovet-564662 --name clyvovetapi-564662 --exec-command "ps aux"

# Deletar um container específico
az container delete --resource-group rg-clyvovet-564662 --name clyvovetapi-564662 --yes
```

### Outros comandos que podem ajudar

```bash
# Informações sobre sua conta
az account show

# Listar Key Vaults deletados (soft-deleted)
az keyvault list-deleted --subscription {ID_DA_SUBSCRICAO} --resource-type vault -o table

# Purgar (deletar permanentemente) um Key Vault — demora
az keyvault purge --subscription {ID_DA_SUBSCRICAO} -n kvclyvovet564662
```

---

## Checklist de entrega

- [ ] Recursos criados via Azure CLI (scripts `01_store-account.sh`, `02_key-vault.sh`, `03_aci-oracledb.sh`, `04_aci-clyvovetapi.sh` versionados no GitHub)
- [ ] `Dockerfile.oracle`, `Dockerfile.api` versionados no GitHub
- [ ] Container do App (`clyvovetapi-564662`) **sem** privilégios de root/admin (garantido pelo `USER app` no `Dockerfile.api`)
- [ ] Comandos completos de `docker build` e `docker push` documentados
- [ ] Banco de dados relacional (Oracle) containerizado com Dockerfile próprio (H2 **não** é aceito)
- [ ] Dados do banco persistidos em Conta de Armazenamento (`stclyvovet564662`)
- [ ] Script DDL das tabelas versionado no GitHub (pode ficar em `init-scripts/` do `Dockerfile.oracle`)
- [ ] Arquivos JSON de teste (GET/POST/PUT/DELETE) versionados no GitHub
- [ ] Vídeo (mín. 720p, com áudio explicativo) mostrando: recursos criados na Azure (ACR, ACI, Storage Account, Key Vault) e evidências de cada operação de CRUD via SELECT no banco
- [ ] Nenhuma credencial sensível exposta no código-fonte (uso do Key Vault)
- [ ] Folha de rosto com nome do grupo, RM e integrantes, link do GitHub e do vídeo