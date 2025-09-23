# 🧪 Testes de Integração - Package.Observability

Este diretório contém testes de integração abrangentes para o `Package.Observability`, testando diferentes cenários de configuração e uso.

## 📁 Estrutura dos Testes

### **1. ConfigurationScenariosTests.cs**
Testa diferentes cenários de configuração do pacote:

- **Desenvolvimento Local**: Apenas console logging e métricas locais
- **Produção Completa**: Métricas + Logs + Tracing + Health Checks
- **Apenas Métricas**: Sem logs e tracing
- **Apenas Logs**: Sem métricas e tracing
- **Apenas Tracing**: Sem métricas e logs
- **Configuração Mínima**: Usando defaults
- **Labels Customizados**: Labels adicionais e do Loki
- **Configuração Inválida**: Tratamento de erros
- **Health Checks**: Verificação de health checks
- **Portas Customizadas**: Diferentes portas do Prometheus

### **2. WithoutLokiScenariosTests.cs**
Testa cenários específicos sem Loki (apenas console logging):

- **Console Only Logging**: Apenas logs no console
- **Console + File Logging**: Logs no console e arquivo
- **Métricas + Console Logging**: Métricas com logs no console
- **Tracing + Console Logging**: Tracing com logs no console
- **Configuração Completa sem Loki**: Todos os componentes exceto Loki
- **Cenário de Desenvolvimento**: Configuração para desenvolvimento
- **Aplicação Simples**: Configuração mínima
- **Tratamento de Erros**: Cenários de erro sem Loki

### **3. CodeConfigurationTests.cs**
Testa configuração por código (Action<ObservabilityOptions>):

- **Desenvolvimento**: Configuração por código para desenvolvimento
- **Produção**: Configuração por código para produção
- **Apenas Métricas**: Configuração por código - apenas métricas
- **Apenas Logs**: Configuração por código - apenas logs
- **Apenas Tracing**: Configuração por código - apenas tracing
- **Labels Customizados**: Configuração por código com labels
- **Porta Customizada**: Configuração por código com porta customizada
- **Configurações de Instrumentação**: Configuração por código com instrumentação
- **Correlation ID**: Configuração por código com Correlation ID
- **Configuração Inválida**: Tratamento de erros na configuração por código

### **4. FallbackAndErrorHandlingTests.cs**
Testa cenários de fallback e tratamento de erros:

- **Configuração Inválida**: ServiceName vazio, porta inválida, etc.
- **ServiceName Ausente**: Tratamento quando ServiceName não é fornecido
- **Porta Inválida**: Tratamento de portas inválidas
- **URL Loki Inválida**: Tratamento de URLs inválidas do Loki
- **Endpoint OTLP Inválido**: Tratamento de endpoints inválidos
- **Log Level Inválido**: Tratamento de níveis de log inválidos
- **Configuração Válida**: Verificação de configuração válida
- **Configuração Parcial**: Uso de defaults para configurações ausentes
- **Configuração Vazia**: Uso de defaults para configuração vazia
- **Configuração com Warnings**: Configuração que gera warnings
- **Labels Inválidos**: Tratamento de labels inválidos
- **ServiceName Longo**: Tratamento de nomes de serviço longos

### **5. HealthChecksTests.cs**
Testa Health Checks do pacote:

- **Registro Automático**: Verificação se health checks são registrados
- **Configuração Válida**: Health checks com configuração válida
- **Configuração Degradada**: Health checks com configuração inválida
- **Checks de Observabilidade**: Verificação de checks específicos
- **Checks de Métricas**: Verificação quando métricas estão habilitadas
- **Checks de Tracing**: Verificação quando tracing está habilitado
- **Checks de Logging**: Verificação quando logging está habilitado
- **Health Checks Customizados**: Integração com health checks customizados
- **Health Checks com Tags**: Health checks com tags específicas
- **Configuração Mínima**: Health checks com configuração mínima
- **Console Only Logging**: Health checks sem Loki

### **6. CustomMetricsTests.cs**
Testa métricas customizadas:

- **Criação de Métricas**: Verificação se métricas customizadas são expostas
- **Labels Corretos**: Verificação de labels nas métricas
- **Diferentes ServiceNames**: Métricas com diferentes nomes de serviço
- **Requisições Concorrentes**: Métricas com requisições concorrentes
- **Cenários de Erro**: Métricas em cenários de erro
- **Serviço de Métricas Customizadas**: Exemplo de serviço que usa métricas

## 🎯 Cenários Testados

### **Configurações por Ambiente**

#### **Desenvolvimento**
```csharp
options.ServiceName = "TestService-Dev";
options.EnableMetrics = true;
options.EnableTracing = false;
options.EnableLogging = true;
options.EnableConsoleLogging = true;
options.LokiUrl = "";
options.OtlpEndpoint = "";
```

#### **Produção**
```csharp
options.ServiceName = "TestService-Prod";
options.EnableMetrics = true;
options.EnableTracing = true;
options.EnableLogging = true;
options.EnableConsoleLogging = false;
options.LokiUrl = "http://loki:3100";
options.OtlpEndpoint = "http://otel-collector:4317";
```

### **Configurações por Componente**

#### **Apenas Métricas**
```csharp
options.EnableMetrics = true;
options.EnableTracing = false;
options.EnableLogging = false;
```

#### **Apenas Logs**
```csharp
options.EnableMetrics = false;
options.EnableTracing = false;
options.EnableLogging = true;
options.EnableConsoleLogging = true;
```

#### **Apenas Tracing**
```csharp
options.EnableMetrics = false;
options.EnableTracing = true;
options.EnableLogging = false;
```

### **Configurações Flexíveis**

#### **Sem Loki (Console Only)**
```csharp
options.EnableLogging = true;
options.EnableConsoleLogging = true;
options.LokiUrl = "";
```

#### **Com Labels Customizados**
```csharp
options.AdditionalLabels.Add("environment", "test");
options.AdditionalLabels.Add("version", "1.0.0");
options.LokiLabels.Add("app", "test-app");
```

## 🔧 Como Executar os Testes

### **Executar Todos os Testes**
```bash
dotnet test tests/Package.Observability.IntegrationTests/Package.Observability.IntegrationTests.csproj
```

### **Executar Testes Específicos**
```bash
# Apenas cenários de configuração
dotnet test --filter "ConfigurationScenariosTests"

# Apenas cenários sem Loki
dotnet test --filter "WithoutLokiScenariosTests"

# Apenas configuração por código
dotnet test --filter "CodeConfigurationTests"

# Apenas fallback e tratamento de erros
dotnet test --filter "FallbackAndErrorHandlingTests"

# Apenas health checks
dotnet test --filter "HealthChecksTests"

# Apenas métricas customizadas
dotnet test --filter "CustomMetricsTests"
```

### **Executar com Logs Detalhados**
```bash
dotnet test --logger "console;verbosity=normal"
```

## 📊 Cobertura dos Testes

### **Cenários de Configuração**
- ✅ Configuração por appsettings.json
- ✅ Configuração por código (Action<ObservabilityOptions>)
- ✅ Configuração por variáveis de ambiente
- ✅ Configuração mínima (defaults)
- ✅ Configuração completa (produção)

### **Componentes Testados**
- ✅ Métricas (Prometheus)
- ✅ Logs (Serilog + Console/Loki)
- ✅ Tracing (OpenTelemetry + OTLP)
- ✅ Health Checks
- ✅ Validação de Configuração
- ✅ Tratamento de Erros

### **Cenários de Uso**
- ✅ Desenvolvimento local
- ✅ Staging
- ✅ Produção
- ✅ Apenas console logging
- ✅ Apenas métricas
- ✅ Apenas logs
- ✅ Apenas tracing
- ✅ Configuração completa

### **Tratamento de Erros**
- ✅ Configuração inválida
- ✅ URLs inválidas
- ✅ Portas inválidas
- ✅ Níveis de log inválidos
- ✅ Labels inválidos
- ✅ Fallback para configuração mínima

## 🚀 Exemplos de Uso

### **1. Teste de Configuração Básica**
```csharp
[Fact]
public async Task DevelopmentConfiguration_ShouldHaveConsoleLoggingAndMetricsOnly()
{
    using var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var config = new Dictionary<string, string?>
                {
                    ["Observability:ServiceName"] = "TestService-Dev",
                    ["Observability:EnableMetrics"] = "true",
                    ["Observability:EnableTracing"] = "false",
                    ["Observability:EnableLogging"] = "true",
                    ["Observability:EnableConsoleLogging"] = "true",
                    ["Observability:LokiUrl"] = "",
                    ["Observability:OtlpEndpoint"] = ""
                };
                configBuilder.AddInMemoryCollection(config);
            });
        });

    var client = factory.CreateClient();
    var response = await client.GetAsync("/WeatherForecast");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### **2. Teste de Configuração por Código**
```csharp
[Fact]
public async Task CodeConfiguration_Development_ShouldWork()
{
    using var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddObservability(options =>
                {
                    options.ServiceName = "TestService-Dev-Code";
                    options.EnableMetrics = true;
                    options.EnableTracing = false;
                    options.EnableLogging = true;
                    options.EnableConsoleLogging = true;
                    options.LokiUrl = "";
                    options.OtlpEndpoint = "";
                });
            });
        });

    var client = factory.CreateClient();
    var response = await client.GetAsync("/WeatherForecast");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### **3. Teste de Métricas Customizadas**
```csharp
[Fact]
public async Task CustomMetrics_ShouldBeExposed_WhenCreated()
{
    using var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTransient<CustomMetricsService>();
            });
        });

    var client = factory.CreateClient();
    var response = await client.GetAsync("/WeatherForecast");
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var metricsResponse = await client.GetAsync("/metrics");
    metricsResponse.IsSuccessStatusCode.Should().BeTrue();
    var metrics = await metricsResponse.Content.ReadAsStringAsync();
    metrics.Should().Contain("custom_requests_total");
}
```

## 🎯 Benefícios dos Testes

### **1. Validação de Configuração**
- Verifica se diferentes configurações funcionam corretamente
- Testa cenários de erro e fallback
- Valida configuração por código e por arquivo

### **2. Flexibilidade**
- Demonstra como usar apenas os componentes necessários
- Testa cenários sem Loki
- Valida configurações por ambiente

### **3. Robustez**
- Testa tratamento de erros
- Valida fallbacks
- Verifica health checks

### **4. Exemplos Práticos**
- Mostra como usar o pacote em diferentes cenários
- Demonstra configurações por código
- Exemplos de métricas customizadas

## 📚 Documentação Relacionada

- **[Guia de Uso Completo](../../docs/usage-guide.md)**
- **[Exemplos de Configuração](../../docs/configuration-examples.md)**
- **[FAQ](../../docs/faq.md)**
- **[Exemplo Sem Loki](../../examples/without-loki-example.md)**

---

**Os testes de integração demonstram a flexibilidade e robustez do `Package.Observability` em diferentes cenários de uso!** 🚀
