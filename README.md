# CP4-DEVOPS- — ClyvoVet

Checkpoint 2º Semestre — Containers em Nuvem (ACR/ACI)

Deploy de uma API .NET + Banco de Dados Oracle como containers na Azure, utilizando **Docker** (build/run separados, sem Compose) para build e testes locais, e **Azure Container Registry (ACR)** + **Azure Container Instances (ACI)** para o registro e execução em nuvem.

## Pré-requisitos

- Docker instalado e em execução
- Azure CLI instalada (`az`)
- Conta ativa no Microsoft Azure
- Git

## Dados do projeto

| Item | Valor |
|---|---|
| RM | `564662` |
| Localização | `canadacentral` |
| Resource Group | `rg-clyvovet-564662` |
| ACR | `clyvovet564662` |

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


### 1. Login na Azure (na sua máquina Windows)

```powershell
az login
az account show
```

### 2. Criar o Resource Group

```powershell
az group create --name rg-clyvovet-564662 --location canadacentral
```

### 3. Criar a VM Linux (Ubuntu)

```powershell
az vm create `
  --resource-group rg-clyvovet-564662 `
  --name vm-clyvovet-564662 `
  --image Ubuntu2204 `
  --size Standard_B2s `
  --admin-username azureuser `
  --generate-ssh-keys `
  --public-ip-sku Standard
```

> `--generate-ssh-keys` cria (ou reaproveita) um par de chaves SSH em `~/.ssh/id_rsa` automaticamente — não precisa de senha.

### 4. Liberar a porta 8080 no Network Security Group (opcional)

Só necessário se você quiser testar a API pelo navegador/Postman direto do seu PC, apontando para a VM:

```powershell
az vm open-port --resource-group rg-clyvovet-564662 --name vm-clyvovet-564662 --port 8080
```

### 5. Pegar o IP público da VM

```powershell
az vm show -d --resource-group rg-clyvovet-564662 --name vm-clyvovet-564662 --query publicIps -o tsv
```

### 6. Conectar via SSH

```powershell
ssh azureuser@<IP_PUBLICO_DA_VM>
```

> Daqui em diante, **todos os comandos abaixo rodam dentro da VM**, na sessão SSH.

## Parte 2 — Preparar a VM (instalar Docker e Azure CLI)

### 7. Instalar o Docker Engine

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Permite rodar docker sem sudo
sudo usermod -aG docker $USER
newgrp docker
```

Teste:

```bash
docker --version
```

### 8. Instalar o Azure CLI

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### 9. Login na Azure (dentro da VM)

Como a VM não tem navegador, use o login por código de dispositivo:

```bash
az login --use-device-code
```

Ele mostra um código e um link — abra o link no navegador do seu PC, cole o código e confirme.

## Parte 3 — Build e testes locais (dentro da VM)

### 10. Clonar o repositório

```bash
git clone https://github.com/AndreBellandi/CP4-DEVOPS-.git
cd CP4-DEVOPS-/ClyvoVetApi-main
```

### 11. Criar uma rede Docker local

```bash
docker network create clyvovet-net
```

### 12. Build das imagens localmente

```bash
docker build -f Dockerfile.oracle -t oracle-db-564662 .
docker build -f Dockerfile.api -t clyvovet-api-564662 .
```

### 13. Rodar o banco Oracle localmente

```bash
docker run -d \
  --name oracle-db \
  --network clyvovet-net \
  -e ORACLE_PASSWORD=020207 \
  -p 1521:1521 \
  -v oracle_data:/opt/oracle/oradata \
  oracle-db-564662
```

Aguarde o banco inicializar (pode levar 1-2 minutos na primeira vez):

```bash
docker logs -f oracle-db
```

### 14. Rodar a API localmente

```bash
docker run -d \
  --name clyvovet-api \
  --network clyvovet-net \
  -e ASPNETCORE_URLS=http://+:8080 \
  -p 8080:8080 \
  clyvovet-api-564662
```

### 15. Testar localmente (dentro da VM)

```bash
curl -X GET http://localhost:8080/api/transacoes
```

Se você liberou a porta 8080 no passo 4, também pode testar do seu PC apontando para o IP público da VM:

```powershell
curl http://<IP_PUBLICO_DA_VM>:8080/api/transacoes
```

### 16. Derrubar o ambiente local (quando terminar os testes)

```bash
docker stop clyvovet-api oracle-db
docker rm clyvovet-api oracle-db
docker network rm clyvovet-net
```

## Parte 4 — Registrar as imagens no ACR

### 17. Criar o Azure Container Registry (ACR)

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

### 18. Obter credenciais do ACR

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

### 19. Login no ACR

```bash
az acr login --name clyvovet564662
```

ou

```bash
docker login $LOGIN_SERVER \
  -u $ADMIN_USERNAME \
  -p $ADMIN_PASSWORD
```

### 20. Tag e push das imagens para o ACR

```bash
docker image ls

docker tag oracle-db-564662 $LOGIN_SERVER/oracle-db-564662:v1
docker push $LOGIN_SERVER/oracle-db-564662:v1

docker tag clyvovet-api-564662 $LOGIN_SERVER/clyvovet-api-564662:v1
docker push $LOGIN_SERVER/clyvovet-api-564662:v1
```

### 21. Conferir imagens registradas no ACR

```bash
az acr repository list --name clyvovet564662 --output table
```

Comandos úteis adicionais:

```bash
# Listar as tags (imagens) de um repositório
az acr repository show-tags --name clyvovet564662 --repository clyvovet-api-564662

# Mostrar manifesto
az acr manifest list-metadata --registry clyvovet564662 --name clyvovet-api-564662

# Limpar imagens antigas do ACR
az acr purge --name clyvovet564662 --filter 'clyvovet-api-564662:.*' --ago 7d --untagged

# Detalhes de um repositório
az acr repository show --name clyvovet564662 --repository clyvovet-api-564662

# Habilitar usuário administrador
az acr update --name clyvovet564662 --admin-enabled true
```

### 22. (Opcional) Remover imagens locais após o push

```bash
docker rmi $LOGIN_SERVER/oracle-db-564662:v1
docker rmi $LOGIN_SERVER/clyvovet-api-564662:v1
```

## Parte 5 — Deploy em Azure Container Instances (ACI)

### 23. Deploy do banco Oracle em ACI

```bash
az container create \
  --resource-group rg-clyvovet-564662 \
  --name oracle-db-564662 \
  --image $LOGIN_SERVER/oracle-db-564662:v1 \
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
az container logs --resource-group rg-clyvovet-564662 --name oracle-db-564662

FQDN_DB=$(az container show --resource-group rg-clyvovet-564662 --name oracle-db-564662 \
  --query ipAddress.fqdn --output tsv)
```

### 24. Deploy da API .NET em ACI

```bash
az container create \
  --resource-group rg-clyvovet-564662 \
  --name clyvovet-api-564662 \
  --image $LOGIN_SERVER/clyvovet-api-564662:v1 \
  --registry-login-server $LOGIN_SERVER \
  --registry-username $ADMIN_USERNAME \
  --registry-password $ADMIN_PASSWORD \
  --ports 8080 \
  --dns-name-label clyvovet-api-564662 \
  --environment-variables ASPNETCORE_URLS=http://+:8080
```

Verificação:

```bash
az container logs --resource-group rg-clyvovet-564662 --name clyvovet-api-564662

FQDN_API=$(az container show --resource-group rg-clyvovet-564662 --name clyvovet-api-564662 \
  --query ipAddress.fqdn --output tsv)

curl -X GET http://$FQDN_API:8080/api/transacoes
```

### 25. Testes de CRUD em nuvem

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

### 26. Comandos úteis de operação/troubleshooting

```bash
# Logs do container
az container logs --resource-group rg-clyvovet-564662 --name clyvovet-api-564662

# Logs com streaming
az container logs --resource-group rg-clyvovet-564662 --name clyvovet-api-564662 --follow

# Sessão interativa
az container exec --resource-group rg-clyvovet-564662 --name clyvovet-api-564662 --exec-command "/bin/bash"

# Verificar processos
az container exec --resource-group rg-clyvovet-564662 --name clyvovet-api-564662 --exec-command "ps aux"

# Deletar um container específico
az container delete --resource-group rg-clyvovet-564662 --name clyvovet-api-564662 --yes
```

### 27. Outros comandos que podem ajudar

```bash
# Informações sobre sua conta
az account show

# Listar Key Vaults deletados (soft-deleted)
az keyvault list-deleted --subscription {ID_DA_SUBSCRICAO} --resource-type vault -o table

# Purgar (deletar permanentemente) um Key Vault — demora
az keyvault purge --subscription {ID_DA_SUBSCRICAO} -n {NOME_DO_VAULT}
```

### 28. (Opcional) Apagar a VM depois de terminar

A VM só serviu como ambiente de build/push — depois que as imagens estão no ACR e os ACIs estão rodando, ela não precisa mais existir (e continua sendo cobrada enquanto ligada):

```powershell
az vm deallocate --resource-group rg-clyvovet-564662 --name vm-clyvovet-564662
# ou, para apagar de vez:
az vm delete --resource-group rg-clyvovet-564662 --name vm-clyvovet-564662 --yes
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