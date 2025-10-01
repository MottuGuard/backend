# Projeto MottuGuard

## Descrição do Projeto

Sistema de localização de motos dentro de um pátio utilizando etiquetas UWB (Ultra Wideband).
O backend é implementado em **ASP.NET Core** com:

* API RESTful com Controllers
* Autenticação via **JWT** (ASP.NET Core Identity)
* Persistência em banco **PostgreSQL** via **Entity Framework Core** e **Migrations**
* Documentação **OpenAPI (Swagger)**

**Principais funcionalidades:**

* Gerenciamento de motos (`Moto`)
* Cadastro e status de tags UWB (`UwbTag`)
* Cadastro de âncoras UWB (`UwbAnchor`)
* Registro de medições UWB (`UwbMeasurement`)
* Histórico de posições calculadas (`PositionRecord`)
* Registro e login de usuários

> ⚙️ **Migrations automáticas**
> No startup a aplicação executa `Database.Migrate()` com retry, aplicando migrations pendentes automaticamente (útil em containers/ACI quando o DB demora a subir).

---

## Rotas da API (resumo)

### Auth

* **POST** `/api/Auth/register` — `{ Username, Email, Name, Password }`
* **POST** `/api/Auth/login` — `{ Username, Password }` → retorna JWT

### Motos

* **GET** `/api/Motos` — filtros: `status`, `modelo`, `page`, `pageSize`
* **GET** `/api/Motos/{id}`
* **POST** `/api/Motos`
* **PUT** `/api/Motos/{id}`
* **DELETE** `/api/Motos/{id}`

### UwbAnchors / UwbTags / UwbMeasurements / PositionRecords

Mesma ideia: GET (com filtros), GET por id, POST, PUT, DELETE.

---

## Pré-requisitos

* [.NET SDK 9](https://dotnet.microsoft.com/download)
* **Docker** e **Docker Compose** (para rodar local)
* **Azure CLI** (para deploy em ACR + ACI)
* (Opcional) `jq` para scripts de teste

---

## local com Docker Compose


**Subir local:**

```bash
docker compose up -d
# API: http://localhost:8080
```

**Logs & testes:**

```bash
docker logs -f api-mottuguard
docker exec -it db-mottuguard psql -U postgres -d aquarumprotector_db -c '\dt'
```

**Derrubar:**

```bash
docker compose down -v
```

---

## Migrations (CLI – opcional)

> Em dev, subindo via compose, as migrations já aplicam no startup.
> Se quiser gerar/rodar manualmente:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add Initial -c MottuContext -o Data/Migrations
dotnet ef database update -c MottuContext
```

---

## Build & Deploy na Azure (ACR + ACI)

A entrega utiliza **Azure Container Registry (ACR)** para armazenar a imagem e **Azure Container Instance (ACI)** para executar a API. O **PostgreSQL** também roda em ACI (imagem oficial `postgres:17`).

### Scripts

```
/scripts
  build.sh   # build da imagem dentro do ACR (az acr build)
  deploy.sh  # cria ACI do Postgres e ACI da API, injeta envs e aponta a connection string
```
---

## Testes rápidos (cURL)

```bash
BASE=http://localhost:8080 # ou http://<FQDN>:8080 no ACI

# Cadastro e login
curl -s -X POST $BASE/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","email":"admin@test.com","name":"Admin","password":"Admin!234"}'

TOKEN=$(curl -s -X POST $BASE/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin!234"}' | jq -r .token)

# Exemplo de POST em Motos
curl -s -X POST $BASE/api/Motos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"chassi":"CHS123","placa":"ABC1D23","modelo":"MottuSportESD","status":"Disponivel"}'
```

---

## Docker (build/run manual)

```bash
# Build
docker build -t mottuguard/backend:dev .

# Run (exemplo com Postgres local)
docker run -d --name api -p 8080:8080 \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e Jwt__Key="uqW8EXYt+3WOsDntgbG5Jt68rNTMmKZwpawNRcMIkSY=" \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=mottu;Username=postgres;Password=SenhaMuitoForte1234!" \
  mottuguard/backend:dev
```

## Limpeza

* **Local**: `docker compose down -v`
* **Azure** (rápido): `az group delete -n rg-challenge-mottu --yes`

---
