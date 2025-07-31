# 🧪 Testes

Este documento descreve a estratégia de testes e como executar os testes do Package.Observability.

## 📋 Visão Geral dos Testes

O Package.Observability possui uma suíte abrangente de testes unitários que garantem a qualidade e confiabilidade do código.

### 🎯 Cobertura de Testes

| Componente | Descrição | Cobertura |
|------------|-----------|-----------|
| `ObservabilityOptions` | Configurações e binding | ✅ Completa |
| `ObservabilityStartupExtensions` | Métodos de extensão DI | ✅ Completa |
| `ActivitySourceFactory` | Factory para tracing | ✅ Completa |
| `ObservabilityMetrics` | Factory para métricas | ✅ Completa |

## 🚀 Executando os Testes

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 ou VS Code

### Via Linha de Comando

```bash
# Executar todos os testes
dotnet test

# Executar testes com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes em modo verboso
dotnet test --verbosity normal

# Executar testes de uma classe específica
dotnet test --filter "ClassName=ObservabilityOptionsTests"

# Executar um teste específico
dotnet test --filter "TestMethodName=ObservabilityOptions_DefaultValues_ShouldBeCorrect"
```

### Via Visual Studio

1. Abrir o projeto no Visual Studio 2022
2. Ir em `Test` > `Run All Tests`
3. Visualizar resultados no Test Explorer

### Via VS Code

1. Instalar extensão `C# Dev Kit`
2. Abrir Command Palette (`Ctrl+Shift+P`)
3. Executar comando `Test: Run All Tests`

## 🧪 Estrutura dos Testes

### Organização

```
Package.Observability.Tests/
├── ObservabilityOptionsTests.cs          # Testes de configuração
├── ObservabilityStartupExtensionsTests.cs # Testes de DI
├── ActivitySourceFactoryTests.cs         # Testes de tracing
├── ObservabilityMetricsTests.cs          # Testes de métricas
├── appsettings.test.json                 # Configuração para testes
└── Package.Observability.Tests.csproj    # Projeto de testes
```

### Padrões de Teste

Todos os testes seguem o padrão **AAA (Arrange, Act, Assert)**:

```csharp
[TestMethod]
public void Method_Scenario_ExpectedResult()
{
    // Arrange - Configurar dados de teste
    var options = new ObservabilityOptions();
    
    // Act - Executar o método sendo testado
    options.ServiceName = "TestService";
    
    // Assert - Verificar o resultado
    options.ServiceName.Should().Be("TestService");
}
```

## 📊 Tipos de Testes

### 1. Testes de Configuração

**Arquivo**: `ObservabilityOptionsTests.cs`

**O que testa**:
- Valores padrão das propriedades
- Binding de configuração via `appsettings.json`
- Binding de configuração via memória
- Configurações parciais
- Validação de propriedades

**Exemplo**:
```csharp
[TestMethod]
public void ObservabilityOptions_DefaultValues_ShouldBeCorrect()
{
    var options = new ObservabilityOptions();
    
    options.ServiceName.Should().Be("DefaultService");
    options.PrometheusPort.Should().Be(9090);
    options.EnableMetrics.Should().BeTrue();
}
```

### 2. Testes de Dependency Injection

**Arquivo**: `ObservabilityStartupExtensionsTests.cs`

**O que testa**:
- Registro de serviços
- Configuração via `IConfiguration`
- Configuração via `Action<ObservabilityOptions>`
- Seções customizadas de configuração
- Validação de parâmetros nulos

**Exemplo**:
```csharp
[TestMethod]
public void AddObservability_WithConfiguration_ShouldRegisterServices()
{
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();
    
    services.AddObservability(configuration);
    
    var serviceProvider = services.BuildServiceProvider();
    var options = serviceProvider.GetService<IOptions<ObservabilityOptions>>();
    options.Should().NotBeNull();
}
```

### 3. Testes de Tracing

**Arquivo**: `ActivitySourceFactoryTests.cs`

**O que testa**:
- Criação de `ActivitySource`
- Singleton pattern (mesma instância para mesmos parâmetros)
- Thread safety
- Gerenciamento de recursos (`DisposeAll`)
- Validação de parâmetros
- Criação de `Activity`

**Exemplo**:
```csharp
[TestMethod]
public void GetOrCreate_WithSameParameters_ShouldReturnSameInstance()
{
    var source1 = ActivitySourceFactory.GetOrCreate("TestService", "1.0.0");
    var source2 = ActivitySourceFactory.GetOrCreate("TestService", "1.0.0");
    
    source1.Should().BeSameAs(source2);
}
```

### 4. Testes de Métricas

**Arquivo**: `ObservabilityMetricsTests.cs`

**O que testa**:
- Criação de `Meter`
- Criação de diferentes tipos de métricas (Counter, Histogram, Gauge, UpDownCounter)
- Singleton pattern para meters
- Thread safety
- Gerenciamento de recursos
- Validação de parâmetros

**Exemplo**:
```csharp
[TestMethod]
public void CreateCounter_WithValidParameters_ShouldReturnCounter()
{
    var counter = ObservabilityMetrics.CreateCounter<int>("TestService", "test_counter", "count", "Description");
    
    counter.Should().NotBeNull();
    counter.Name.Should().Be("test_counter");
    counter.Unit.Should().Be("count");
}
```

## 🔧 Ferramentas de Teste

### Frameworks Utilizados

- **MSTest**: Framework de teste principal
- **FluentAssertions**: Assertions mais legíveis
- **Moq**: Mocking (quando necessário)

### Dependências de Teste

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
<PackageReference Include="MSTest.TestFramework" Version="3.1.1" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.69" />
```

## 📈 Cobertura de Código

### Gerando Relatório de Cobertura

```bash
# Instalar ferramenta de relatório
dotnet tool install -g dotnet-reportgenerator-globaltool

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório HTML
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Visualizando Cobertura

1. Executar os comandos acima
2. Abrir `coverage-report/index.html` no navegador
3. Navegar pelos arquivos para ver cobertura detalhada

### Metas de Cobertura

- **Mínima**: 80%
- **Ideal**: 90%+
- **Classes principais**: 95%+

## 🧪 Testes de Integração

### Configuração para Testes

```csharp
public static class TestHelper
{
    public static IServiceCollection AddTestObservability(this IServiceCollection services)
    {
        return services.AddObservability(options =>
        {
            options.ServiceName = "TestService";
            options.EnableMetrics = false;
            options.EnableTracing = false;
            options.EnableLogging = true;
            options.EnableConsoleLogging = false;
            options.LokiUrl = "";
            options.OtlpEndpoint = "";
        });
    }
}
```

### Exemplo de Teste de Integração

```csharp
[TestMethod]
public void Integration_AddObservability_ShouldConfigureAllServices()
{
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.test.json")
        .Build();

    // Act
    services.AddObservability(configuration);
    var serviceProvider = services.BuildServiceProvider();

    // Assert
    var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
    
    options.Should().NotBeNull();
    loggerFactory.Should().NotBeNull();
}
```

## 🚀 Automação de Testes

### GitHub Actions

Os testes são executados automaticamente no CI/CD:

```yaml
- name: Run tests
  run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

- name: Upload coverage reports
  uses: codecov/codecov-action@v3
  with:
    files: '**/coverage.cobertura.xml'
```

### Pre-commit Hooks

Para executar testes antes de cada commit:

```bash
# .git/hooks/pre-commit
#!/bin/sh
dotnet test --no-build
if [ $? -ne 0 ]; then
    echo "Tests failed. Commit aborted."
    exit 1
fi
```

## 🐛 Debugging de Testes

### Visual Studio

1. Colocar breakpoint no teste
2. Clicar com botão direito no teste
3. Selecionar "Debug Test"

### VS Code

1. Instalar extensão `C# Dev Kit`
2. Colocar breakpoint no teste
3. Usar `Debug Test` no CodeLens

### Linha de Comando

```bash
# Executar teste específico em modo debug
dotnet test --filter "TestMethodName" --logger "console;verbosity=detailed"
```

## 📝 Boas Práticas

### 1. Nomenclatura de Testes

```csharp
// Padrão: MethodName_Scenario_ExpectedResult
[TestMethod]
public void GetOrCreate_WithNullServiceName_ShouldThrow()

[TestMethod]
public void AddObservability_WithValidConfiguration_ShouldRegisterServices()
```

### 2. Isolamento de Testes

```csharp
[TestCleanup]
public void Cleanup()
{
    // Limpar estado compartilhado
    ActivitySourceFactory.DisposeAll();
    ObservabilityMetrics.DisposeAll();
}
```

### 3. Assertions Claras

```csharp
// ✅ Bom - Específico e claro
options.ServiceName.Should().Be("ExpectedService");

// ❌ Ruim - Genérico
Assert.IsTrue(options.ServiceName == "ExpectedService");
```

### 4. Dados de Teste

```csharp
// ✅ Bom - Constantes descritivas
const string TestServiceName = "TestService";
const int TestPort = 9999;

// ❌ Ruim - Magic numbers
var options = new ObservabilityOptions { PrometheusPort = 1234 };
```

## 🔍 Troubleshooting

### Testes Falhando

1. **Verificar configuração**: Garantir que `appsettings.test.json` existe
2. **Limpar build**: `dotnet clean && dotnet build`
3. **Verificar dependências**: `dotnet restore`
4. **Executar teste isolado**: `dotnet test --filter "TestMethodName"`

### Performance de Testes

```bash
# Executar testes em paralelo
dotnet test --parallel

# Executar apenas testes rápidos
dotnet test --filter "Category!=Integration"
```

### Logs de Debug

```csharp
[TestMethod]
public void DebugTest()
{
    // Usar Console.WriteLine para debug
    Console.WriteLine($"Test value: {value}");
    
    // Ou usar ITestOutputHelper (xUnit)
    // _output.WriteLine($"Test value: {value}");
}
```

## 📚 Recursos Adicionais

- [MSTest Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)