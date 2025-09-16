# Guia de Desenvolvimento

Guia para contribuir e desenvolver o Package.Observability.

## 🚀 Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 ou VS Code
- Docker (para testes de integração)
- Git

## 📁 Estrutura do Projeto

```
Package.Observability/
├── Package.Observability/           # Código fonte do pacote
│   ├── ActivitySourceFactory.cs     # Factory para ActivitySource
│   ├── ObservabilityMetrics.cs      # Factory para métricas
│   ├── ObservabilityOptions.cs      # Opções de configuração
│   └── ObservabilityStartupExtensions.cs # Extensões de configuração
├── examples/                        # Exemplos de uso
│   └── WebApi.Example/              # Exemplo de Web API
├── tests/                          # Testes
│   └── Package.Observability.IntegrationTests/ # Testes de integração
├── docs/                           # Documentação
├── observability/                  # Configurações de observabilidade
└── docker-compose.yml              # Stack de observabilidade
```

## 🔧 Configuração do Ambiente

### 1. Clone o Repositório

```bash
git clone https://github.com/your-org/Package.Observability.git
cd Package.Observability
```

### 2. Restaure as Dependências

```bash
dotnet restore
```

### 3. Execute os Testes

```bash
dotnet test
```

### 4. Execute o Exemplo

```bash
cd examples/WebApi.Example
dotnet run
```

## 🧪 Executando Testes

### Testes Unitários

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "ClassName=ServiceRegistrationTests"
```

### Testes de Integração

```bash
# Executar testes de integração
dotnet test tests/Package.Observability.IntegrationTests/
```

### Testes com Docker

```bash
# Iniciar stack de observabilidade
docker-compose up -d

# Executar testes
dotnet test

# Parar stack
docker-compose down
```

## 🏗️ Build e Pack

### Build Local

```bash
# Build em Debug
dotnet build

# Build em Release
dotnet build --configuration Release
```

### Pack do Pacote

```bash
# Gerar pacote NuGet
dotnet pack --configuration Release

# Gerar pacote com versão específica
dotnet pack --configuration Release --version-suffix "beta1"
```

## 📝 Padrões de Código

### 1. Convenções de Nomenclatura

- **Classes**: PascalCase (ex: `ObservabilityOptions`)
- **Métodos**: PascalCase (ex: `AddObservability`)
- **Propriedades**: PascalCase (ex: `ServiceName`)
- **Campos privados**: _camelCase (ex: `_meters`)
- **Constantes**: UPPER_CASE (ex: `DEFAULT_SERVICE_NAME`)

### 2. Documentação XML

```csharp
/// <summary>
/// Adiciona serviços de observabilidade à coleção de serviços
/// </summary>
/// <param name="services">A coleção de serviços</param>
/// <param name="configuration">A configuração da aplicação</param>
/// <param name="sectionName">Nome da seção de configuração</param>
/// <returns>A coleção de serviços para encadeamento</returns>
public static IServiceCollection AddObservability(
    this IServiceCollection services, 
    IConfiguration configuration, 
    string sectionName = "Observability")
```

### 3. Tratamento de Erros

```csharp
public static Meter GetOrCreateMeter(string serviceName, string? version = null)
{
    if (string.IsNullOrWhiteSpace(serviceName))
        throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));

    // Resto da implementação...
}
```

### 4. Thread Safety

```csharp
private static readonly object _lock = new();

public static Meter GetOrCreateMeter(string serviceName, string? version = null)
{
    // Verificação inicial sem lock
    if (_meters.TryGetValue(key, out var existingMeter))
        return existingMeter;

    lock (_lock)
    {
        // Verificação dupla dentro do lock
        if (_meters.TryGetValue(key, out existingMeter))
            return existingMeter;

        // Criação da instância
        var meter = new Meter(serviceName, version ?? "1.0.0");
        _meters[key] = meter;
        return meter;
    }
}
```

## 🔍 Análise de Código

### 1. Linting

```bash
# Executar análise de código
dotnet build --verbosity normal

# Verificar warnings
dotnet build --verbosity detailed
```

### 2. Code Coverage

```bash
# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
```

### 3. Análise Estática

```bash
# Instalar SonarAnalyzer
dotnet add package SonarAnalyzer.CSharp

# Executar análise
dotnet build
```

## 🚀 Processo de Desenvolvimento

### 1. Criar Branch

```bash
git checkout -b feature/nova-funcionalidade
```

### 2. Desenvolver

- Escreva código seguindo os padrões
- Adicione testes para nova funcionalidade
- Atualize documentação se necessário
- Execute testes localmente

### 3. Commit

```bash
git add .
git commit -m "feat: adiciona nova funcionalidade X"
```

### 4. Push e Pull Request

```bash
git push origin feature/nova-funcionalidade
```

### 5. Code Review

- Aguarde revisão do código
- Faça ajustes solicitados
- Execute testes após mudanças

## 📋 Checklist de Desenvolvimento

### Antes de Commitar

- [ ] Código compila sem erros
- [ ] Todos os testes passam
- [ ] Cobertura de testes adequada (>80%)
- [ ] Documentação XML atualizada
- [ ] Tratamento de erros adequado
- [ ] Thread safety verificado
- [ ] Performance testada

### Antes do Pull Request

- [ ] Branch atualizada com main
- [ ] Testes de integração executados
- [ ] Documentação atualizada
- [ ] Exemplos atualizados
- [ ] Changelog atualizado

## 🧪 Adicionando Novos Testes

### 1. Teste Unitário

```csharp
[Fact]
public void Should_Create_Meter_With_Valid_ServiceName()
{
    // Arrange
    var serviceName = "TestService";
    
    // Act
    var meter = ObservabilityMetrics.GetOrCreateMeter(serviceName);
    
    // Assert
    Assert.NotNull(meter);
    Assert.Equal(serviceName, meter.Name);
}
```

### 2. Teste de Integração

```csharp
[Fact]
public async Task Should_Expose_Metrics_Endpoint()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/metrics");
    
    // Assert
    response.IsSuccessStatusCode.Should().BeTrue();
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");
}
```

### 3. Teste de Performance

```csharp
[Fact]
public void Should_Handle_Concurrent_Meter_Creation()
{
    // Arrange
    var tasks = new List<Task<Meter>>();
    var serviceName = "ConcurrentTest";
    
    // Act
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(() => ObservabilityMetrics.GetOrCreateMeter(serviceName)));
    }
    
    var meters = Task.WhenAll(tasks).Result;
    
    // Assert
    var uniqueMeters = meters.Distinct().Count();
    Assert.Equal(1, uniqueMeters); // Deve retornar a mesma instância
}
```

## 📚 Adicionando Documentação

### 1. Atualizar README

- Adicione nova funcionalidade na seção de características
- Atualize exemplos se necessário
- Atualize tabela de configuração

### 2. Adicionar Exemplos

```csharp
// Adicione exemplos em docs/examples.md
// Mostre casos de uso comuns
// Inclua configurações específicas
```

### 3. Atualizar API Reference

```csharp
// Documente novos métodos em docs/api-reference.md
// Inclua parâmetros e retornos
// Adicione exemplos de uso
```

## 🔧 Configuração de CI/CD

### 1. GitHub Actions

```yaml
name: CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
    - name: Pack
      run: dotnet pack --no-build --configuration Release
```

### 2. Azure DevOps

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    configuration: '$(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    configuration: '$(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Pack'
  inputs:
    command: 'pack'
    configuration: '$(buildConfiguration)'
```

## 🚀 Release

### 1. Atualizar Versão

```xml
<PropertyGroup>
  <PackageVersion>1.1.0</PackageVersion>
</PropertyGroup>
```

### 2. Atualizar Changelog

```markdown
## [1.1.0] - 2024-01-15

### Added
- Nova funcionalidade X
- Suporte para Y

### Changed
- Melhoria na performance
- Atualização de dependências

### Fixed
- Bug fix Z
```

### 3. Criar Tag

```bash
git tag -a v1.1.0 -m "Release version 1.1.0"
git push origin v1.1.0
```

### 4. Publicar no NuGet

```bash
dotnet nuget push Package.Observability.1.1.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## 📚 Recursos Adicionais

- [Guia de Início Rápido](getting-started.md)
- [Configuração](configuration.md)
- [Exemplos](examples.md)
- [API Reference](api-reference.md)
- [Troubleshooting](troubleshooting.md)
