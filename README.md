# CP4-DEVOPS- — ClyvoVet

Checkpoint 2º Semestre — Containers em Nuvem (ACR/ACI)

Deploy de uma API .NET + Banco de Dados Oracle como containers na Azure, utilizando **Docker** (build/run separados, sem Compose) para build e testes locais, e **Azure Container Registry (ACR)** + **Azure Container Instances (ACI)** para o registro e execução em nuvem.

## Pré-requisitos

- Docker instalado e em execução
- Azure CLI instalada (`az`)
- Conta ativa no Microsoft Azure
- Git

## Variáveis do projeto

Antes de começar, defina as variáveis abaixo no seu terminal. Todos os comandos deste README usam essas variáveis — ajuste apenas aqui.

```bash
export RM=564662
export LOCATION=canadacentral         # altere conforme sua política/região
export RESOURCE_GROUP=rg-clyvovet-$RM
export ACR_NAME=clyvovet${RM}   # nomes de ACR só aceitam letras/números
```

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

> Ajuste a versão do SDK/runtime (`8.0`) para a versão do .NET usada no projeto, e o nome da DLL no `ENTRYPOINT` para o nome real do assembly gerado (geralmente `<NomeDoProjeto>.dll`).

## How To

### 1. Clonar o repositório

```bash
git clone https://github.com/AndreBellandi/CP4-DEVOPS-.git
cd CP4-DEVOPS-/ClyvoVetApi-main
```

### 2. Criar uma rede Docker local

```bash
docker network create clyvovet-net
```

### 3. Build das imagens localmente

```bash
docker build -f Dockerfile.oracle -t oracle-db-$RM .
docker build -f Dockerfile.api -t clyvovet-api-$RM .
```

### 4. Rodar o banco Oracle localmente

```bash
docker run -d \
  --name oracle-db \
  --network clyvovet-net \
  -e ORACLE_PASSWORD=020207 \
  -p 1521:1521 \
  -v oracle_data:/opt/oracle/oradata \
  oracle-db-$RM
```

Aguarde o banco inicializar (pode levar 1-2 minutos na primeira vez):

```bash
docker logs -f oracle-db
```

### 5. Rodar a API localmente

```bash
docker run -d \
  --name clyvovet-api \
  --network clyvovet-net \
  -e ASPNETCORE_URLS=http://+:8080 \
  -p 8080:8080 \
  clyvovet-api-$RM
```

### 6. Testar localmente

```bash
curl -X GET http://localhost:8080/api/transacoes
```

### 7. Derrubar o ambiente local (quando terminar os testes)

```bash
docker stop clyvovet-api oracle-db
docker rm clyvovet-api oracle-db
docker network rm clyvovet-net
```

### 8. Login na Azure

```bash
az login
az account show
```

### 9. Criar o Resource Group

```bash
az group create --name $RESOURCE_GROUP --location $LOCATION
```

Caso ocorra erro de subscription:

```bash
az account list -o table
az account set --subscription "<nome ou id da subscription>"
```

### 10. Criar o Azure Container Registry (ACR)

```bash
az provider register --namespace Microsoft.ContainerRegistry

az acr create \
    --resource-group $RESOURCE_GROUP \
    --name $ACR_NAME \
    --sku Standard \
    --location $LOCATION \
    --public-network-enabled true \
    --admin-enabled true
```

### 11. Obter credenciais do ACR

```bash
LOGIN_SERVER=$(az acr show --name $ACR_NAME \
                           --resource-group $RESOURCE_GROUP \
                           --query loginServer --output tsv)
echo ""
echo "Login Server: $LOGIN_SERVER"
echo ""

ADMIN_USERNAME=$(az acr credential show --name $ACR_NAME \
                                        --resource-group $RESOURCE_GROUP \
                                        --query username --output tsv) && \
ADMIN_PASSWORD=$(az acr credential show --name $ACR_NAME \
                                        --resource-group $RESOURCE_GROUP \
                                 --query passwords[0].value --output tsv) && \
echo "Username: $ADMIN_USERNAME" && echo "Password: $ADMIN_PASSWORD"
```

### 12. Login no ACR

```bash
az acr login --name $ACR_NAME
```

ou

```bash
docker login $LOGIN_SERVER \
  -u $ADMIN_USERNAME \
  -p $ADMIN_PASSWORD
```

### 13. Tag e push das imagens para o ACR

```bash
docker image ls

docker tag oracle-db-$RM $LOGIN_SERVER/oracle-db-$RM:v1
docker push $LOGIN_SERVER/oracle-db-$RM:v1

docker tag clyvovet-api-$RM $LOGIN_SERVER/clyvovet-api-$RM:v1
docker push $LOGIN_SERVER/clyvovet-api-$RM:v1
```

### 14. Conferir imagens registradas no ACR

```bash
az acr repository list --name $ACR_NAME --output table
```

Comandos úteis adicionais:

```bash
# Listar as tags (imagens) de um repositório
az acr repository show-tags --name $ACR_NAME --repository clyvovet-api-$RM

# Mostrar manifesto
az acr manifest list-metadata --registry $ACR_NAME --name clyvovet-api-$RM

# Limpar imagens antigas do ACR
az acr purge --name $ACR_NAME --filter 'clyvovet-api-'$RM':.*' --ago 7d --untagged

# Detalhes de um repositório
az acr repository show --name $ACR_NAME --repository clyvovet-api-$RM

# Habilitar usuário administrador
az acr update --name $ACR_NAME --admin-enabled true
```

### 15. (Opcional) Remover imagens locais após o push

```bash
docker rmi $LOGIN_SERVER/oracle-db-$RM:v1
docker rmi $LOGIN_SERVER/clyvovet-api-$RM:v1
```

### 16. Deploy do banco Oracle em ACI

```bash
az container create \
  --resource-group $RESOURCE_GROUP \
  --name oracle-db-$RM \
  --image $LOGIN_SERVER/oracle-db-$RM:v1 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ADMIN_USERNAME \
  --registry-password $ADMIN_PASSWORD \
  --ports 1521 \
  --environment-variables ORACLE_PASSWORD=020207 \
  --azure-file-volume-account-name <NOME_DA_STORAGE_ACCOUNT> \
  --azure-file-volume-account-key <CHAVE_DA_STORAGE_ACCOUNT> \
  --azure-file-volume-share-name oracle-data \
  --azure-file-volume-mount-path /opt/oracle/oradata
```

> O volume `--azure-file-volume-*` persiste os dados do banco em uma Conta de Armazenamento, conforme exigido no checkpoint.

Verificação:

```bash
az container logs --resource-group $RESOURCE_GROUP --name oracle-db-$RM

FQDN_DB=$(az container show --resource-group $RESOURCE_GROUP --name oracle-db-$RM \
  --query ipAddress.fqdn --output tsv)
```

### 17. Deploy da API .NET em ACI

```bash
az container create \
  --resource-group $RESOURCE_GROUP \
  --name clyvovet-api-$RM \
  --image $LOGIN_SERVER/clyvovet-api-$RM:v1 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ADMIN_USERNAME \
  --registry-password $ADMIN_PASSWORD \
  --ports 8080 \
  --dns-name-label clyvovet-api-$RM \
  --environment-variables ASPNETCORE_URLS=http://+:8080
```

Verificação:

```bash
az container logs --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM

FQDN_API=$(az container show --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM \
  --query ipAddress.fqdn --output tsv)

curl -X GET http://$FQDN_API:8080/api/transacoes
```

### 18. Testes de CRUD em nuvem

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

### 19. Comandos úteis de operação/troubleshooting

```bash
# Logs do container
az container logs --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM

# Logs com streaming
az container logs --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM --follow

# Sessão interativa
az container exec --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM --exec-command "/bin/bash"

# Verificar processos
az container exec --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM --exec-command "ps aux"

# Deletar um container específico
az container delete --resource-group $RESOURCE_GROUP --name clyvovet-api-$RM --yes
```

### 20. Outros comandos que podem ajudar

```bash
# Informações sobre sua conta
az account show

# Listar Key Vaults deletados (soft-deleted)
az keyvault list-deleted --subscription {ID_DA_SUBSCRICAO} --resource-type vault -o table

# Purgar (deletar permanentemente) um Key Vault — demora
az keyvault purge --subscription {ID_DA_SUBSCRICAO} -n {NOME_DO_VAULT}
```

## Checklist de entrega

- [ ] Recursos criados via Azure CLI (scripts versionados no GitHub)
- [ ] `Dockerfile.oracle`, `Dockerfile.api` e demais YAML versionados no GitHub
- [ ] Container do App **sem** privilégios de root/admin (garantido pelo `USER app` no `Dockerfile.api`)
- [ ] Comandos completos de `docker build` e `docker push` documentados
- [ ] Banco de dados relacional containerizado com Dockerfile próprio (H2 **não** é aceito)
- [ ] Dados do banco persistidos em Conta de Armazenamento
- [ ] Script DDL das tabelas versionado no GitHub (pode ficar em `init-scripts/` do `Dockerfile.oracle`)
- [ ] Arquivos JSON de teste (GET/POST/PUT/DELETE) versionados no GitHub
- [ ] Vídeo (mín. 720p, com áudio explicativo) mostrando: recursos criados na Azure (ACR, ACI, Conta de Armazenamento) e evidências de cada operação de CRUD via SELECT no banco
- [ ] Nenhuma credencial sensível exposta no código-fonte
- [ ] Folha de rosto com nome do grupo, RM e integrantes, link do GitHub e do vídeo

## Licença / Créditos

Projeto acadêmico — FIAP, Tecnologia em Desenvolvimento de Sistemas, disciplina DevOps Tools & Cloud Computing. Baseado no checkpoint "1º Checkpoint 2º Semestre – Containers em Nuvem (ACR/ACI)" (Prof. João Menk).