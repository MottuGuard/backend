# Projeto MottuGuard

## Descrição do Projeto

Sistema de localização de motos dentro de um pátio utilizando etiquetas UWB (Ultra Wideband). O backend é implementado em ASP.NET Core com:

* API RESTful com Controllers
* Autenticação via JWT (ASP.NET Core Identity)
* Persistência em banco Oracle via Entity Framework Core e Migrations
* Documentação OpenAPI (Swagger)

Funcionalidades principais:

* Gerenciamento de motos (`Moto`)
* Cadastro e status de tags UWB (`UwbTag`)
* Cadastro de âncoras UWB (`UwbAnchor`)
* Registro de medições UWB (`UwbMeasurement`)
* Histórico de posições calculadas (`PositionRecord`)
* Registro e login de usuários

## Rotas da API

### Auth

* **POST** `/api/Auth/register`
  Registra novo usuário. Recebe JSON `{ Username, Email, Name, Password }`.

* **POST** `/api/Auth/login`
  Autentica usuário. Recebe JSON `{ Username, Password }`. Retorna JWT e tempo de expiração.

### Motos

* **GET** `/api/Motos`
  Filtra por `status`, `modelo`, paginação `page` e `pageSize`.

* **GET** `/api/Motos/{id}`
  Retorna moto por ID.

* **POST** `/api/Motos`
  Cria nova moto. Recebe JSON da entidade `Moto`.

* **PUT** `/api/Motos/{id}`
  Atualiza moto existente.

* **DELETE** `/api/Motos/{id}`
  Remove moto.

### UwbAnchors

* **GET** `/api/UwbAnchors`
  Filtra por `nameContains`, página e tamanho de página.

* **GET** `/api/UwbAnchors/{id}`
  Retorna âncora por ID.

* **POST** `/api/UwbAnchors`
  Cria nova âncora.

* **PUT** `/api/UwbAnchors/{id}`
  Atualiza âncora.

* **DELETE** `/api/UwbAnchors/{id}`
  Remove âncora.

### UwbTags

* **GET** `/api/UwbTags`
  Filtra por `status`.

* **GET** `/api/UwbTags/{id}`
  Retorna tag por ID.

* **POST** `/api/UwbTags`
  Cria nova tag.

* **PUT** `/api/UwbTags/{id}`
  Atualiza tag.

* **DELETE** `/api/UwbTags/{id}`
  Remove tag.

### UwbMeasurements

* **GET** `/api/UwbMeasurements`
  Filtra por `tagId`, `anchorId`, intervalo `from` e `to`, página e tamanho de página.

* **GET** `/api/UwbMeasurements/{id}`
  Retorna medição por ID.

* **POST** `/api/UwbMeasurements`
  Registra nova medição.

* **PUT** `/api/UwbMeasurements/{id}`
  Atualiza medição.

* **DELETE** `/api/UwbMeasurements/{id}`
  Remove medição.

### PositionRecords

* **GET** `/api/PositionRecords`
  Filtra por `motoId`, intervalo `from` e `to`, página e tamanho de página.

* **GET** `/api/PositionRecords/{id}`
  Retorna registro por ID.

* **POST** `/api/PositionRecords`
  Cria novo registro de posição.

* **PUT** `/api/PositionRecords/{id}`
  Atualiza registro de posição.

* **DELETE** `/api/PositionRecords/{id}`
  Remove registro de posição.

## Instalação

### Pré-requisitos

* [.NET SDK 9](https://dotnet.microsoft.com/download)
* Banco de dados Oracle acessível
* Ferramenta `dotnet-ef` instalada globalmente:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Clonar o Repositório

```bash
git clone https://github.com/MottuGuard/backend.git
cd uwb-moto-tracker
```

### Configurar Conexão e JWT

No arquivo `appsettings.json`, ajuste a seção abaixo:

```json
"ConnectionStrings": {
  "DefaultConnection": "User Id=USER;Password=PASSWORD;Data Source=//HOST:PORT/SERVICE"
},
"Jwt": {
  "Key": "SUA_CHAVE_SECRETA_LONGA",
  "Issuer": "SeuProjeto",
  "Audience": "SeuProjetoClient"
}
```

### Criar e Aplicar Migrations

```bash
dotnet ef migrations add InitialCreate --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

### Executar a Aplicação

```bash
dotnet run
```

Acesse `https://localhost:7095/swagger/index.html` para a interface Swagger UI e testes de API.

---

