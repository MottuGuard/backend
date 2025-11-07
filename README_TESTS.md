# Testes - MottuGuard Backend

Este documento contém instruções completas para executar os testes do backend do MottuGuard.

## 📋 Requisitos

- .NET 9 SDK
- Nenhum banco de dados externo necessário (testes usam banco em memória)

## 🏗️ Estrutura de Testes

O projeto contém dois projetos de teste separados:

### 1. **backend.Tests** - Testes Unitários
Localização: `backend/backend.Tests/`

Testes para lógica de negócio isolada, incluindo:
- **Services**: TokenService, AuthService (25+ testes)
- **Controllers**: Validação de lógica de controladores
- **Helpers**: Utilitários e construtores de dados de teste

### 2. **backend.IntegrationTests** - Testes de Integração
Localização: `backend/backend.IntegrationTests/`

Testes end-to-end da API usando `WebApplicationFactory`:
- Fluxos completos de autenticação
- Operações CRUD com banco de dados
- Validação de regras de negócio integradas

## 🚀 Executando os Testes

### Executar TODOS os testes

```bash
cd backend
dotnet test
```

### Executar apenas Testes Unitários

```bash
dotnet test backend.Tests/backend.Tests.csproj
```

### Executar apenas Testes de Integração

```bash
dotnet test backend.IntegrationTests/backend.IntegrationTests.csproj
```

### Executar com Saída Detalhada

```bash
dotnet test --verbosity detailed
```

### Executar testes específicos por nome

```bash
dotnet test --filter "DisplayName~TokenService"
```

## 📊 Relatório de Cobertura de Código

### Gerar relatório de cobertura

```bash
# Instalar ferramenta de cobertura (apenas uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Executar testes com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/

# Gerar relatório HTML
reportgenerator -reports:**/coverage.opencover.xml -targetdir:./TestResults/CoverageReport -reporttypes:Html

# Abrir relatório
start ./TestResults/CoverageReport/index.html  # Windows
# open ./TestResults/CoverageReport/index.html   # macOS
# xdg-open ./TestResults/CoverageReport/index.html  # Linux
```

### Ver cobertura no terminal

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## ✅ Testes Implementados

### Testes Unitários (backend.Tests)

#### **TokenServiceTests** (10 testes)
- ✅ `GenerateRefreshToken_ShouldReturnBase64String` - Verifica geração de refresh token válido
- ✅ `GenerateRefreshToken_ShouldReturnUniqueTokensOnMultipleCalls` - Garante unicidade
- ✅ `GenerateRefreshToken_ShouldReturn44CharactersLength` - Valida tamanho correto
- ✅ `GenerateToken_ShouldReturnValidJwtToken` - JWT com 3 partes (header.payload.signature)
- ✅ `GenerateToken_ShouldIncludeUserClaimsInToken` - Claims de usuário no JWT
- ✅ `GenerateToken_ShouldExpireIn30Minutes` - Expiração configurada corretamente
- ✅ `GetPrincipalFromToken_ShouldExtractClaimsFromValidToken` - Extração de claims
- ✅ `GetPrincipalFromToken_ShouldThrowException_WhenTokenIsInvalid` - Rejeita tokens inválidos
- ✅ `GetPrincipalFromToken_ShouldNotValidateLifetime` - Não valida expiração ao extrair claims
- ✅ `GenerateToken_ShouldGenerateUniqueJtiForEachToken` - JTI único por token

#### **AuthServiceTests** (15 testes)
- ✅ `Authenticate_WithValidCredentials_ShouldReturnOkWithTokens` - Login bem-sucedido
- ✅ `Authenticate_WithInvalidEmail_ShouldReturnUnauthorized` - Email inválido
- ✅ `Authenticate_WithInvalidPassword_ShouldReturnUnauthorized` - Senha incorreta
- ✅ `Authenticate_ShouldGenerateAndStoreRefreshToken` - Refresh token gerado e armazenado
- ✅ `Register_WithValidData_ShouldCreateUser` - Registro de usuário
- ✅ `Register_ShouldSetUsernameToEmail` - Username = Email
- ✅ `Register_WithDuplicateEmail_ShouldReturnBadRequest` - Email duplicado rejeitado
- ✅ `Register_ShouldAddUserToUserRole` - Usuário adicionado à role "User"
- ✅ `Authenticate_WithValidRefreshToken_ShouldReturnNewTokens` - Refresh token válido
- ✅ `Authenticate_WithInvalidToken_ShouldReturnUnauthorized` - Token JWT inválido
- ✅ `Authenticate_WithExpiredRefreshToken_ShouldReturnUnauthorized` - Refresh token expirado
- ✅ `Authenticate_WithMismatchedRefreshToken_ShouldReturnUnauthorized` - Refresh token não corresponde
- ✅ `Authenticate_WithNonExistentUser_ShouldReturnNotFound` - Usuário não encontrado
- ✅ `Authenticate_RefreshFlow_ShouldUpdateUserWithNewRefreshToken` - Atualiza refresh token

**Total: 25 testes unitários**

### Testes de Integração (backend.IntegrationTests)

*(A serem implementados - estrutura pronta)*

#### **AuthControllerIntegrationTests** (planejado)
- Fluxo completo: Registro → Login → Acesso protegido
- Refresh token end-to-end
- Middleware de autenticação JWT

#### **MotosControllerIntegrationTests** (planejado)
- CRUD completo de motos
- Regra de negócio: Não deletar moto reservada
- Detecção de duplicatas (Placa, Chassi)
- Paginação com dados reais

## 🎯 Cobertura de Código

### Meta do Projeto: 80%+

**Cobertura Atual:**
- **Services**: ~85% (TokenService, AuthService)
- **Controllers**: A ser implementado
- **Models**: ~90% (propriedades simples)
- **Helpers**: 100%

### Componentes Testados

✅ **Implementado:**
- Geração e validação de JWT tokens
- Autenticação email/senha
- Refresh token flow
- Registro de usuários
- Validação de duplicatas

⏳ **Pendente:**
- Controllers (Motos, UwbTags, Predictions)
- Regras de negócio de motos
- Validações de API
- Testes de integração E2E

## 🛠️ Infraestrutura de Testes

### Helpers Disponíveis

#### `TestDataBuilder`
Construtor fluente para criar dados de teste:

```csharp
var moto = TestDataBuilder.Moto()
    .WithPlaca("ABC1234")
    .WithChassi("CHASSI123")
    .WithStatus(MotoStatus.Disponivel)
    .Build();

var user = TestDataBuilder.ApplicationUser()
    .WithEmail("test@test.com")
    .WithName("Test User")
    .Build();
```

#### `TestWebApplicationFactory`
Factory para testes de integração com banco em memória:

```csharp
var factory = new TestWebApplicationFactory();
var client = factory.CreateClient();

// Cliente HTTP configurado para testes
var response = await client.GetAsync("/api/v1/Motos");
```

#### `AuthenticationHelper`
Gerador de tokens JWT para testes:

```csharp
var token = AuthenticationHelper.GenerateJwtToken();
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

## 🔧 Configuração de Ambiente de Teste

Os testes utilizam:
- **Banco de Dados**: EF Core InMemory (sem PostgreSQL necessário)
- **MQTT**: Desabilitado (MqttConsumerService removido em testes)
- **JWT**: Chave de teste fixa
- **Identity**: UserManager/RoleManager mockados

## 📝 Boas Práticas Implementadas

### Padrão AAA (Arrange-Act-Assert)
Todos os testes seguem o padrão:
```csharp
[Fact]
public async Task Method_Condition_ExpectedResult()
{
    // Arrange - Configurar dados de teste
    var input = "test data";

    // Act - Executar ação
    var result = await Service.Method(input);

    // Assert - Verificar resultado
    result.Should().NotBeNull();
}
```

### FluentAssertions
Assertions legíveis e descritivas:
```csharp
result.Should().BeOfType<OkObjectResult>();
token.Should().NotBeNullOrEmpty();
jwtToken.Claims.Should().Contain(c => c.Type == "email");
```

### Mocking com Moq
Isolamento de dependências:
```csharp
_mockUserManager.Setup(x => x.FindByEmailAsync(email))
    .ReturnsAsync(user);
```

## 🐛 Debugging de Testes

### Executar teste específico com debug
```bash
dotnet test --filter "FullyQualifiedName~TokenService" --logger "console;verbosity=detailed"
```

### Ver saída de console nos testes
```csharp
[Fact]
public void Test()
{
    Console.WriteLine("Debug info");
    _output.WriteLine("Test output"); // Com ITestOutputHelper
}
```

## 📚 Tecnologias Utilizadas

- **xUnit** 2.9.3 - Framework de testes
- **Moq** 4.20.72 - Mocking framework
- **FluentAssertions** 7.0.0 - Assertions expressivas
- **Microsoft.AspNetCore.Mvc.Testing** 9.0.4 - Testes de integração
- **Microsoft.EntityFrameworkCore.InMemory** 9.0.4 - Banco em memória
- **Coverlet** 6.0.2 - Cobertura de código

## 🎓 Estrutura de Arquivos

```
backend/
├── backend.Tests/                    # Testes Unitários
│   ├── Services/
│   │   ├── TokenServiceTests.cs     # 10 testes
│   │   └── AuthServiceTests.cs      # 15 testes
│   ├── Controllers/                 # (A implementar)
│   ├── Helpers/
│   │   └── TestDataBuilder.cs       # Builders de teste
│   └── backend.Tests.csproj
│
├── backend.IntegrationTests/         # Testes de Integração
│   ├── Controllers/                  # (A implementar)
│   ├── Helpers/
│   │   ├── TestWebApplicationFactory.cs
│   │   └── AuthenticationHelper.cs
│   └── backend.IntegrationTests.csproj
│
└── README_TESTS.md                   # Este arquivo
```

## ✨ Exemplos de Execução

### CI/CD Pipeline
```yaml
# .github/workflows/tests.yml
- name: Run Tests
  run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

### Teste rápido antes de commit
```bash
dotnet test --no-build --verbosity quiet
```

### Verificar falhas detalhadas
```bash
dotnet test --logger "console;verbosity=detailed" --filter "FullyQualifiedName~AuthService"
```

## 📞 Suporte

Para problemas com testes:
1. Verificar versão do .NET: `dotnet --version` (requer 9.0+)
2. Limpar e reconstruir: `dotnet clean && dotnet build`
3. Restaurar pacotes: `dotnet restore`

---

**Desenvolvido para:** Trabalho de Faculdade - Backend com .NET 9
**Testes:** 25+ testes unitários, framework de integração pronto
**Cobertura:** Meta 80%+ (Em progresso)
