# ⚙️ Configuração Detalhada

Este documento descreve todas as opções de configuração disponíveis no Package.Observability.

## 📋 Opções de Configuração

### ObservabilityOptions

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `ServiceName` | `string` | `"DefaultService"` | Nome do serviço para identificação em métricas, logs e traces |
| `PrometheusPort` | `int` | `9090` | Porta do endpoint HTTP para métricas Prometheus |
| `EnableMetrics` | `bool` | `true` | Habilita coleta e exportação de métricas |
| `EnableTracing` | `bool` | `true` | Habilita rastreamento distribuído |
| `EnableLogging` | `bool` | `true` | Habilita logs estruturados |
| `LokiUrl` | `string` | `"http://localhost:3100"` | URL do Grafana Loki para agregação de logs |
| `OtlpEndpoint` | `string` | `"http://localhost:4317"` | Endpoint OTLP para exportação de traces |
| `EnableConsoleLogging` | `bool` | `true` | Habilita saída de logs no console |
| `MinimumLogLevel` | `string` | `"Information"` | Nível mínimo de log (Trace, Debug, Information, Warning, Error, Fatal) |
| `EnableCorrelationId` | `bool` | `true` | Habilita geração automática de Correlation ID |
| `AdditionalLabels` | `Dictionary<string, string>` | `{}` | Labels adicionais aplicados a todas as métricas |
| `LokiLabels` | `Dictionary<string, string>` | `{}` | Labels específicos para Loki |
| `EnableRuntimeInstrumentation` | `bool` | `true` | Habilita métricas de runtime .NET (GC, threads, etc.) |
| `EnableHttpClientInstrumentation` | `bool` | `true` | Habilita instrumentação de HTTP Client para requests externos |
| `EnableAspNetCoreInstrumentation` | `bool` | `true` | Habilita instrumentação ASP.NET Core para requests HTTP |

## 🔧 Métodos de Configuração

### 1. Via appsettings.json (Recomendado)

```json
{
  "Observability": {
    "ServiceName": "MeuBot",
    "PrometheusPort": 9090,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "LokiUrl": "http://loki:3100",
    "OtlpEndpoint": "http://jaeger:4317",
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.2.0",
      "team": "backend"
    },
    "LokiLabels": {
      "app": "fishing-bot",
      "component": "worker"
    }
  }
}
```

```csharp
// Program.cs
builder.Services.AddObservability(builder.Configuration);
```

### 2. Via Código

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuBot";
    options.PrometheusPort = 9110;
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.OtlpEndpoint = "http://jaeger:4317";
    options.AdditionalLabels.Add("bot-type", "fishing");
    options.LokiLabels.Add("component", "worker");
});
```

### 3. Via IOptions (Avançado)

```csharp
// Configuração
builder.Services.Configure<ObservabilityOptions>(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    // ... outras configurações
});

// Uso em classe
public class MeuServico
{
    private readonly ObservabilityOptions _options;
    
    public MeuServico(IOptions<ObservabilityOptions> options)
    {
        _options = options.Value;
    }
}
```

## 🌍 Configuração por Ambiente

### appsettings.Development.json

```json
{
  "Observability": {
    "ServiceName": "MeuBot-Dev",
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Debug",
    "LokiUrl": "",
    "OtlpEndpoint": ""
  }
}
```

### appsettings.Production.json

```json
{
  "Observability": {
    "ServiceName": "MeuBot-Prod",
    "EnableConsoleLogging": false,
    "MinimumLogLevel": "Information",
    "LokiUrl": "https://loki.company.com",
    "OtlpEndpoint": "https://jaeger.company.com:4317",
    "AdditionalLabels": {
      "environment": "production",
      "datacenter": "us-east-1"
    }
  }
}
```

## 🔐 Configuração com Secrets

### User Secrets (Desenvolvimento)

```bash
dotnet user-secrets set "Observability:LokiUrl" "http://localhost:3100"
dotnet user-secrets set "Observability:OtlpEndpoint" "http://localhost:4317"
```

### Azure Key Vault (Produção)

```csharp
builder.Configuration.AddAzureKeyVault(/* configuração */);

// Os secrets serão automaticamente injetados na configuração
```

### Variáveis de Ambiente

```bash
export Observability__ServiceName="MeuBot"
export Observability__LokiUrl="http://loki:3100"
export Observability__OtlpEndpoint="http://jaeger:4317"
```

```csharp
// Automaticamente carregado
builder.Services.AddObservability(builder.Configuration);
```

## 🎛️ Configurações Avançadas

### Configuração Condicional

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuBot";
    
    // Habilitar métricas apenas em produção
    options.EnableMetrics = builder.Environment.IsProduction();
    
    // Logs detalhados em desenvolvimento
    options.MinimumLogLevel = builder.Environment.IsDevelopment() ? "Debug" : "Information";
    
    // Loki apenas se a URL estiver configurada
    var lokiUrl = builder.Configuration["Observability:LokiUrl"];
    options.LokiUrl = string.IsNullOrEmpty(lokiUrl) ? "" : lokiUrl;
});
```

### Configuração Dinâmica

```csharp
// Recarregar configuração em runtime
builder.Services.Configure<ObservabilityOptions>(
    builder.Configuration.GetSection("Observability"));

// Monitorar mudanças
builder.Services.PostConfigure<ObservabilityOptions>(options =>
{
    // Validações ou ajustes dinâmicos
    if (string.IsNullOrEmpty(options.ServiceName))
        options.ServiceName = Environment.MachineName;
});
```

### Configuração Customizada por Seção

```csharp
// Usar seção diferente
builder.Services.AddObservability(builder.Configuration, "MyObservability");
```

```json
{
  "MyObservability": {
    "ServiceName": "CustomService"
  }
}
```

## 🧪 Configuração para Testes

### appsettings.Test.json

```json
{
  "Observability": {
    "ServiceName": "MeuBot-Test",
    "EnableMetrics": false,
    "EnableTracing": false,
    "EnableLogging": true,
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Warning",
    "LokiUrl": "",
    "OtlpEndpoint": ""
  }
}
```

### Configuração para Testes Unitários

```csharp
// TestHelper.cs
public static class TestHelper
{
    public static IServiceCollection AddTestObservability(this IServiceCollection services)
    {
        return services.AddObservability(options =>
        {
            options.ServiceName = "Test";
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

## ✅ Validação de Configuração

### Validação Automática

```csharp
builder.Services.AddOptions<ObservabilityOptions>()
    .Bind(builder.Configuration.GetSection("Observability"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### Validação Customizada

```csharp
builder.Services.AddOptions<ObservabilityOptions>()
    .Bind(builder.Configuration.GetSection("Observability"))
    .Validate(options =>
    {
        if (options.EnableMetrics && options.PrometheusPort <= 0)
            return false;
        
        if (options.EnableLogging && string.IsNullOrEmpty(options.ServiceName))
            return false;
            
        return true;
    }, "Configuração de observabilidade inválida")
    .ValidateOnStart();
```

## 📊 Configurações de Performance

### Otimização para Alto Volume

```json
{
  "Observability": {
    "EnableRuntimeInstrumentation": false,
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true,
    "MinimumLogLevel": "Warning"
  }
}
```

### Configuração Mínima (Recursos Limitados)

```json
{
  "Observability": {
    "EnableMetrics": true,
    "EnableTracing": false,
    "EnableLogging": true,
    "EnableConsoleLogging": true,
    "LokiUrl": ""
  }
}
```

## 🔍 Debug de Configuração

### Verificar Configuração Carregada

```csharp
public class DiagnosticService
{
    public DiagnosticService(IOptions<ObservabilityOptions> options, ILogger<DiagnosticService> logger)
    {
        var config = options.Value;
        logger.LogInformation("Observability Config: {@Config}", config);
    }
}
```

### Logs de Configuração

```csharp
builder.Services.PostConfigure<ObservabilityOptions>(options =>
{
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Config");
    logger.LogInformation("ServiceName: {ServiceName}", options.ServiceName);
    logger.LogInformation("EnableMetrics: {EnableMetrics}", options.EnableMetrics);
    logger.LogInformation("PrometheusPort: {PrometheusPort}", options.PrometheusPort);
});
```