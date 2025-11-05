# MottuGuard

Sistema de gestão de frotas de motocicletas com rastreamento indoor utilizando tecnologia UWB (Ultra Wideband) e BLE (Bluetooth Low Energy).

## Visão Geral

**MottuGuard** é uma solução completa para monitoramento e gestão de frotas de motocicletas em tempo real, combinando rastreamento de localização indoor de alta precisão com uma plataforma de gestão robusta. O sistema permite acompanhar a localização das motocicletas, receber alertas de eventos (movimento, geofencing, offline) e gerenciar o status da frota através de aplicativo mobile e APIs.

### Principais Funcionalidades

- Rastreamento em tempo real de motocicletas via tecnologia UWB
- Gestão de status da frota (Disponível, Reservada, Em Manutenção, Bloqueada, Perdida)
- Notificações em tempo real via SignalR
- Alertas de geofencing e detecção de movimento
- Histórico de posições e eventos
- API REST completa para integração
- Aplicativo mobile para gestão da frota
- Dashboard web para monitoramento IoT

### Equipe

- **Gabriel Augusto Fernandes** - RM99711
- **Jaqueline Martins dos Santos** - RM551744
- **Matheus Oliveira da Silva** - RM99792
- **Gilberto Ramos da Silva Neto** - RM551413

### Links

- [Vídeo Demonstração](https://www.youtube.com/watch?v=dQw4w9WgXcQ)
- [Protótipo Figma](https://www.figma.com/design/example)

---

## Arquitetura do Sistema

O MottuGuard é composto por **quatro componentes principais** que trabalham de forma integrada:

1. **Backend** - API ASP.NET Core com PostgreSQL, responsável pela lógica de negócio, autenticação e persistência
2. **Mobile** - Aplicativo React Native (Expo) para gestão da frota por operadores
3. **IoT** - Infraestrutura de simulação MQTT com broker Mosquitto, simuladores de tags UWB e dashboard web
4. **Database** - Suporte multi-database (PostgreSQL principal, Oracle SQL legacy, MongoDB alternativo)

### Diagrama C4 - Contexto do Sistema

```mermaid
C4Context
    title Diagrama de Contexto do Sistema - MottuGuard

    Person(operator, "Operador", "Usuário do sistema que gerencia a frota de motocicletas")
    Person(iot_admin, "Administrador IoT", "Monitora tags e sensores UWB")

    System_Boundary(mottuguard, "MottuGuard") {
        System(mobile_app, "Mobile App", "React Native/Expo<br/>Gestão de frotas e rastreamento")
        System(backend_api, "Backend API", ".NET 9 / ASP.NET Core<br/>Lógica de negócio e autenticação")
        System(iot_infra, "IoT Infrastructure", "Mosquitto MQTT + Simuladores<br/>Tags UWB e telemetria")
        System(database, "Database Layer", "PostgreSQL / Oracle / MongoDB<br/>Persistência de dados")
    }

    System_Ext(uwb_tags, "Tags UWB Físicas", "Hardware UWB anexado às motocicletas (simulado)")

    Rel(operator, mobile_app, "Gerencia frotas", "HTTPS / SignalR")
    Rel(iot_admin, iot_infra, "Monitora sensores", "WebSocket / MQTT")

    Rel(mobile_app, backend_api, "Chamadas API REST", "HTTPS / JWT")
    Rel(backend_api, mobile_app, "Updates em tempo real", "SignalR WebSocket")

    Rel(uwb_tags, iot_infra, "Envia telemetria", "MQTT")
    Rel(iot_infra, backend_api, "Publica mensagens", "MQTT")

    Rel(backend_api, database, "Lê/Escreve dados", "Entity Framework Core")
    Rel(iot_infra, database, "Persiste telemetria", "Ingestor Python (opcional)")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

---

## Arquitetura de Componentes

### Backend - Estrutura Interna

```mermaid
graph TB
    subgraph "Backend API (.NET 9)"
        subgraph "Controllers Layer"
            AuthCtrl[AuthController<br/>Login/Register]
            MotoCtrl[MotosController<br/>CRUD Motos]
            PosCtrl[PositionRecordsController<br/>Histórico]
            TagCtrl[UwbTagsController<br/>Gestão Tags]
            AnchorCtrl[UwbAnchorsController<br/>Gestão Anchors]
        end

        subgraph "Services Layer"
            AuthSvc[AuthService<br/>Autenticação]
            TokenSvc[TokenService<br/>JWT Generation]
            MqttSvc[MqttConsumerService<br/>Background Service]
        end

        subgraph "SignalR Hub"
            MottuHub[MottuHub<br/>Real-time Events]
        end

        subgraph "Data Layer"
            EFCore[Entity Framework Core<br/>Code-First]
            DbContext[MottuContext<br/>DbSets]
            Migrations[Migrations<br/>Auto-apply on startup]
        end

        subgraph "Models/Entities"
            Moto[Moto]
            UwbTag[UwbTag]
            UwbAnchor[UwbAnchor]
            PositionRecord[PositionRecord]
            UwbMeasurement[UwbMeasurement]
            Event[Event]
            User[ApplicationUser]
        end
    end

    subgraph "External Systems"
        PostgreSQL[(PostgreSQL<br/>Database)]
        MqttBroker[Mosquitto<br/>MQTT Broker]
        Clients[Clients<br/>Mobile/Web]
    end

    AuthCtrl --> AuthSvc
    AuthSvc --> TokenSvc
    MotoCtrl --> DbContext
    PosCtrl --> DbContext
    TagCtrl --> DbContext
    AnchorCtrl --> DbContext

    MqttSvc -->|Subscribe mottu/*| MqttBroker
    MqttSvc -->|Persist data| DbContext
    MqttSvc -->|Broadcast updates| MottuHub

    MottuHub -->|WebSocket| Clients

    DbContext --> EFCore
    EFCore --> Moto
    EFCore --> UwbTag
    EFCore --> UwbAnchor
    EFCore --> PositionRecord
    EFCore --> UwbMeasurement
    EFCore --> Event
    EFCore --> User

    EFCore -->|SQL queries| PostgreSQL
    Migrations -.->|Auto-migrate| PostgreSQL

    style MqttSvc fill:#ff9,stroke:#333,stroke-width:2px
    style MottuHub fill:#9f9,stroke:#333,stroke-width:2px
```

### Tópicos MQTT - Estrutura de Mensagens

```mermaid
graph LR
    subgraph "MQTT Topic Tree"
        Root[mottu/]

        subgraph "UWB Topics"
            UWB[uwb/TAG_ID/]
            Position[position<br/>x, y, timestamp]
            Ranging[ranging<br/>distances to anchors]
        end

        subgraph "Sensor Topics"
            Motion[motion/TAG_ID<br/>movement events]
            Status[status/TAG_ID<br/>online/offline/battery]
        end

        subgraph "Event Topics"
            EventTopic[event/TAG_ID<br/>geofence, alerts]
        end

        subgraph "Command Topics"
            Commands[act/TAG_ID/cmd<br/>find_on, lock_on, etc]
        end
    end

    Root --> UWB
    Root --> Motion
    Root --> Status
    Root --> EventTopic
    Root --> Commands

    UWB --> Position
    UWB --> Ranging

    subgraph "Publishers"
        TagSim[Tag Simulators<br/>Python]
        Dashboard[IoT Dashboard<br/>JavaScript]
    end

    subgraph "Subscribers"
        BackendSvc[Backend<br/>MqttConsumerService]
        DashboardSub[Dashboard<br/>WebSocket]
        Ingestor[Ingestor<br/>Python optional]
    end

    TagSim -->|Publish| Position
    TagSim -->|Publish| Ranging
    TagSim -->|Publish| Motion
    TagSim -->|Publish| Status
    TagSim -->|Publish| EventTopic

    Dashboard -->|Publish| Commands

    Position -.->|Subscribe| BackendSvc
    Ranging -.->|Subscribe| BackendSvc
    Motion -.->|Subscribe| BackendSvc
    Status -.->|Subscribe| BackendSvc
    EventTopic -.->|Subscribe| BackendSvc

    Position -.->|Subscribe| DashboardSub
    Ranging -.->|Subscribe| DashboardSub

    Position -.->|Subscribe| Ingestor

    style TagSim fill:#f96,stroke:#333,stroke-width:2px
    style BackendSvc fill:#9cf,stroke:#333,stroke-width:2px
```

---

## Fluxos de Dados

### Fluxo de Autenticação

```mermaid
sequenceDiagram
    actor User as Usuário
    participant Mobile as Mobile App
    participant API as Backend API
    participant Auth as AuthService
    participant Token as TokenService
    participant DB as PostgreSQL

    User->>Mobile: Insere credenciais
    Mobile->>API: POST /api/Auth/login<br/>{username, password}

    API->>Auth: ValidateCredentials()
    Auth->>DB: Query ApplicationUser
    DB-->>Auth: User data

    alt Credenciais válidas
        Auth->>Token: GenerateToken(user)
        Token-->>Auth: JWT token
        Auth-->>API: Success + token
        API-->>Mobile: 200 OK {token, user}
        Mobile->>Mobile: Store token
        Mobile-->>User: Login sucesso
    else Credenciais inválidas
        Auth-->>API: Unauthorized
        API-->>Mobile: 401 Unauthorized
        Mobile-->>User: Erro de login
    end

    Note over Mobile,API: Requisições subsequentes usam<br/>"Authorization: Bearer {token}"
```

### Fluxo de Rastreamento em Tempo Real

```mermaid
sequenceDiagram
    participant Tag as Tag UWB Simulada
    participant Mosquitto as MQTT Broker
    participant Backend as MqttConsumerService
    participant DB as PostgreSQL
    participant Hub as SignalR Hub
    participant Mobile as Mobile App
    participant Dashboard as IoT Dashboard

    Note over Tag: Tag detecta nova posição<br/>via trilateração

    Tag->>Mosquitto: PUBLISH mottu/uwb/tag01/position<br/>{x: 5.2, y: 3.8, ts: ...}

    Mosquitto->>Backend: Message received
    Mosquitto->>Dashboard: Message received (WS)

    Backend->>Backend: Parse JSON payload
    Backend->>DB: Find UwbTag by Eui64
    DB-->>Backend: Tag + Moto relation

    Backend->>DB: INSERT PositionRecord<br/>UPDATE Moto.LastX, LastY, LastSeenAt
    DB-->>Backend: Success

    Backend->>Hub: BroadcastPositionUpdate(motoId, x, y)
    Hub->>Mobile: ReceivePositionUpdate event

    Dashboard->>Dashboard: Update map visualization
    Mobile->>Mobile: Update moto marker on map

    Note over Mobile,Dashboard: Atualização em tempo real<br/>latência < 500ms

    opt Geofence violation detected
        Backend->>DB: INSERT Event (geofence_breach)
        Backend->>Hub: BroadcastGeofenceEvent(motoId)
        Hub->>Mobile: ReceiveGeofenceEvent
        Mobile-->>User: Alert notification
    end
```

### Fluxo de Processamento MQTT

```mermaid
sequenceDiagram
    participant Simulator as Tag Simulator<br/>(Python)
    participant Broker as Mosquitto
    participant Consumer as MqttConsumerService<br/>(Background)
    participant DB as PostgreSQL
    participant SignalR as MottuHub

    Note over Consumer: Service starts on backend startup<br/>Subscribes to mottu/#

    loop Every 5-10 seconds
        Simulator->>Broker: PUBLISH mottu/uwb/tag01/ranging<br/>{anchor1: 3.2m, anchor2: 5.1m, ...}
    end

    Broker->>Consumer: Message on mottu/uwb/tag01/ranging

    Consumer->>Consumer: Extract TAG_ID from topic<br/>Parse JSON payload

    Consumer->>DB: SELECT UwbTag WHERE Eui64 = 'tag01'
    DB-->>Consumer: Tag entity

    loop For each anchor distance
        Consumer->>DB: INSERT UwbMeasurement<br/>{TagId, AnchorId, Distance, Timestamp}
    end

    Consumer->>SignalR: SendRangingUpdate(tagId, measurements)
    SignalR-->>Dashboard: Real-time ranging data

    Note over Consumer,DB: Similar flows for:<br/>- mottu/uwb/TAG_ID/position<br/>- mottu/motion/TAG_ID<br/>- mottu/status/TAG_ID<br/>- mottu/event/TAG_ID
```

---

## Modelo de Domínio

### Diagrama de Entidade-Relacionamento

```mermaid
erDiagram
    ApplicationUser ||--o{ Moto : "gerencia"
    Moto ||--o| UwbTag : "possui"
    Moto ||--o{ PositionRecord : "tem histórico"
    Moto ||--o{ Event : "gera"
    UwbTag ||--o{ UwbMeasurement : "realiza"
    UwbTag ||--o{ PositionRecord : "registra"
    UwbAnchor ||--o{ UwbMeasurement : "referência para"

    ApplicationUser {
        string Id PK
        string UserName
        string Email
        string Name
        string PasswordHash
        DateTime CreatedAt
    }

    Moto {
        int Id PK
        string Chassi UK
        string Placa UK
        string Modelo
        string Status
        decimal LastX
        decimal LastY
        DateTime LastSeenAt
        DateTime CreatedAt
    }

    UwbTag {
        int Id PK
        string Eui64 UK
        int MotoId FK
        string Status
        DateTime LastSeenAt
    }

    UwbAnchor {
        int Id PK
        string AnchorId UK
        decimal X
        decimal Y
        decimal Z
        bool IsActive
    }

    UwbMeasurement {
        int Id PK
        int TagId FK
        int AnchorId FK
        decimal Distance
        DateTime Timestamp
    }

    PositionRecord {
        int Id PK
        int MotoId FK
        int TagId FK
        decimal X
        decimal Y
        DateTime Timestamp
    }

    Event {
        int Id PK
        int MotoId FK
        string EventType
        string Description
        DateTime Timestamp
    }
```

### Principais Entidades

- **Moto**: Representa uma motocicleta da frota
  - Status: `Disponivel`, `Reservada`, `EmManutencao`, `Bloqueada`, `Perdida`
  - Armazena última posição conhecida (LastX, LastY, LastSeenAt)

- **UwbTag**: Tag UWB física anexada à motocicleta
  - Identificada por Eui64 (ex: "tag01", "tag02")
  - Relacionada 1:1 com Moto

- **UwbAnchor**: Pontos de referência fixos para trilateração
  - Posição conhecida (X, Y, Z)
  - Seedados no startup (4 anchors por padrão)

- **UwbMeasurement**: Medições de distância brutas
  - Distância da Tag até cada Anchor
  - Base para cálculo de posição

- **PositionRecord**: Histórico de posições calculadas
  - Trail de movimento ao longo do tempo
  - Gerado a partir de trilateração ou diretamente de topic MQTT

- **Event**: Eventos de alto nível
  - Tipos: `offline`, `geofence_breach`, `motion_detected`, `low_battery`

---

## Stack Tecnológico

### Backend
- **.NET 9** - Framework principal
- **ASP.NET Core** - Web API com Controllers
- **PostgreSQL** - Banco de dados principal
- **Entity Framework Core** - ORM com Code-First migrations
- **ASP.NET Core Identity** - Gerenciamento de usuários
- **JWT (JSON Web Tokens)** - Autenticação stateless
- **SignalR** - Comunicação real-time bidirecional
- **MQTTnet** - Cliente MQTT para subscrição de telemetria
- **Swagger/OpenAPI** - Documentação interativa da API
- **Docker** - Containerização

### Mobile
- **React Native** - Framework mobile cross-platform
- **Expo** - Toolchain e SDK
- **TypeScript** - Tipagem estática
- **React Navigation** - Navegação entre telas
- **React Native Paper** - Biblioteca de componentes UI
- **Axios** - Cliente HTTP

### IoT
- **Mosquitto** - MQTT Broker (TCP + WebSockets)
- **Python 3** - Simuladores de tags UWB
- **Paho MQTT** - Biblioteca Python MQTT
- **HTML/JavaScript** - Dashboard web de monitoramento
- **PostgreSQL** (opcional) - Persistência via Ingestor

### Database
- **PostgreSQL** - Banco principal (Backend + IoT)
- **Oracle SQL** - Schema legacy com packages PL/SQL
- **MongoDB** - Schema alternativo NoSQL

### Infraestrutura
- **Docker & Docker Compose** - Desenvolvimento local
- **Azure Container Registry (ACR)** - Registry de imagens
- **Azure Container Instances (ACI)** - Deploy em produção

---

## Início Rápido

### Pré-requisitos

- **Para Backend:**
  - .NET 9 SDK
  - Docker e Docker Compose
  - PostgreSQL (ou via Docker)

- **Para Mobile:**
  - Node.js 18+
  - npm ou yarn
  - Expo CLI
  - Expo Go app (iOS/Android) ou emulador

- **Para IoT:**
  - Docker e Docker Compose
  - Python 3.9+ (para simuladores)

### Setup Rápido - Ambiente Completo

#### 1. Backend API + PostgreSQL

```bash
cd backend
docker compose up -d

# Verificar logs
docker logs -f backend-challenge-mottu

# API disponível em: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

#### 2. Mobile App

```bash
cd mobile
npm install
npm start

# Escanear QR code com Expo Go ou:
npm run android  # Android
npm run ios      # iOS (apenas macOS)
```

**Configuração:** Edite `mobile/src/config/env.ts` com a URL do backend.

#### 3. IoT Infrastructure

```bash
cd iot
docker compose up -d

# Iniciar simuladores de tags
cd simulators
py -m pip install -r requirements.txt
py tag_sim.py tag01  # Terminal 1
py tag_sim.py tag02  # Terminal 2
py tag_sim.py tag03  # Terminal 3

# Dashboard: http://localhost:8081
# MQTT: localhost:1883 (TCP) | ws://localhost:8080 (WebSocket)
```

### Teste Rápido da API

```bash
# Registrar usuário
curl -X POST http://localhost:8080/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","email":"admin@test.com","name":"Admin","password":"Admin!234"}'

# Login (obter token JWT)
curl -X POST http://localhost:8080/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin!234"}'

# Listar motos (usando token)
curl http://localhost:8080/api/Motos \
  -H "Authorization: Bearer {SEU_TOKEN_AQUI}"
```

### Variáveis de Ambiente

**Backend** (`appsettings.json` ou variáveis de ambiente):

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Database=mottu;Username=postgres;Password=postgres"
Jwt__Key="<base64-encoded-key-min-32-chars>"
Jwt__Issuer="MottuGuard"
Jwt__Audience="MottuGuardClients"
Mqtt__Host="localhost"
Mqtt__Port="1883"
```

**Mobile** (`src/config/env.ts`):

```typescript
export const API_URL = 'http://192.168.1.100:8080'; // IP da máquina host
```

**IoT Ingestor** (opcional):

```bash
PG_DSN='dbname=mottu user=postgres password=postgres host=localhost port=5432'
```

---

## Documentação dos Componentes

Cada componente possui documentação detalhada em seu respectivo diretório:

- **[Backend](./backend/README.md)** - API .NET, migrations, controllers, services
- **[Mobile](./mobile/README.md)** - App React Native, estrutura de telas e services
- **[IoT](./iot/README.md)** - Simuladores, dashboard, broker MQTT
- **[Database](./database/README_MONGODB.md)** - Schemas Oracle e MongoDB

---

## Deploy

### Desenvolvimento Local (Docker Compose)

Cada componente possui um `docker-compose.yml` para desenvolvimento local:

```bash
# Backend + PostgreSQL
cd backend && docker compose up -d

# IoT (Mosquitto + Dashboard + PostgreSQL)
cd iot && docker compose up -d
```

### Produção (Azure)

O backend possui scripts para deploy em Azure Container Instances:

```bash
cd backend

# Build e push para Azure Container Registry
./build.sh

# Deploy no Azure (ACI)
./deploy.sh
```

**Recursos criados:**
- Azure Container Registry (ACR)
- Azure Container Instances (ACI) para PostgreSQL
- Azure Container Instances (ACI) para API

---

## API Reference

### Principais Endpoints

**Autenticação:**
- `POST /api/Auth/register` - Registrar novo usuário
- `POST /api/Auth/login` - Login (retorna JWT token)
- `GET /api/Auth/profile` - Perfil do usuário autenticado

**Motocicletas:**
- `GET /api/Motos` - Listar todas as motos
- `GET /api/Motos/{id}` - Detalhes de uma moto
- `POST /api/Motos` - Cadastrar nova moto
- `PUT /api/Motos/{id}` - Atualizar moto
- `DELETE /api/Motos/{id}` - Remover moto
- `GET /api/Motos/{id}/position-history` - Histórico de posições

**Tags UWB:**
- `GET /api/UwbTags` - Listar tags
- `GET /api/UwbTags/{id}` - Detalhes de uma tag
- `POST /api/UwbTags` - Cadastrar tag
- `PUT /api/UwbTags/{id}/assign` - Associar tag a moto

**Posições:**
- `GET /api/PositionRecords` - Histórico de posições
- `GET /api/PositionRecords/moto/{motoId}` - Posições de uma moto específica

**SignalR Hub** (`/mottuHub`):
- `ReceivePositionUpdate` - Atualização de posição em tempo real
- `ReceiveRangingUpdate` - Medições de distância
- `ReceiveGeofenceEvent` - Alertas de geofencing
- `ReceiveMotionEvent` - Eventos de movimento
- `ReceiveStatusUpdate` - Status das tags

**Documentação Completa:** Acesse `/swagger` quando executar o backend em modo Development.

---

## Contribuindo

Este é um projeto acadêmico desenvolvido para a disciplina de **Engenharia de Software** da FIAP.

### Workflow de Desenvolvimento

1. Clone o repositório
2. Crie uma branch para sua feature: `git checkout -b feature/nova-funcionalidade`
3. Faça commit das alterações: `git commit -m 'Add nova funcionalidade'`
4. Push para a branch: `git push origin feature/nova-funcionalidade`
5. Abra um Pull Request

---

## Licença

Este projeto é parte de um trabalho acadêmico e está disponível para fins educacionais.

**Curso:** Análise e Desenvolvimento de Sistemas - FIAP
**Disciplina:** Engenharia de Software
**Ano:** 2024/2025

---

