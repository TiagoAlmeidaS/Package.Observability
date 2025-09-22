# Configuração

Este documento detalha todas as opções de configuração disponíveis no Package.Observability.

## 📋 Opções de Configuração

### ObservabilityOptions

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `ServiceName` | `string` | `"DefaultService"` | Nome do serviço para identificação em métricas, logs e traces |
| `PrometheusPort` | `int` | `9090` | Porta do endpoint de métricas Prometheus |
| `EnableMetrics` | `bool` | `true` | Habilita coleta de métricas |
| `EnableTracing` | `bool` | `true` | Habilita rastreamento distribuído |
| `EnableLogging` | `bool` | `true` | Habilita logs estruturados |
| `LokiUrl` | `string` | `"http://localhost:3100"` | URL do Grafana Loki para logs |
| `OtlpEndpoint` | `string` | `"http://localhost:4317"` | Endpoint OTLP para traces |
| `EnableConsoleLogging` | `bool` | `true` | Habilita logs no console |
| `MinimumLogLevel` | `string` | `"Information"` | Nível mínimo de log (Debug, Information, Warning, Error, Fatal) |
| `EnableCorrelationId` | `bool` | `true` | Habilita Correlation ID automático |
| `AdditionalLabels` | `Dictionary<string, string>` | `{}` | Labels adicionais para métricas |
| `LokiLabels` | `Dictionary<string, string>` | `{}` | Labels customizados para Loki |
| `EnableRuntimeInstrumentation` | `bool` | `true` | Habilita métricas de runtime .NET |
| `EnableHttpClientInstrumentation` | `bool` | `true` | Habilita instrumentação HTTP Client |
| `EnableAspNetCoreInstrumentation` | `bool` | `true` | Habilita instrumentação ASP.NET Core |

## 🔧 Configuração via appsettings.json

### Configuração Básica

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true
  }
}
```

### Configuração Completa

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "PrometheusPort": 9090,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "LokiUrl": "http://localhost:3100",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0",
      "team": "backend",
      "region": "us-east-1"
    },
    "LokiLabels": {
      "app": "minha-aplicacao",
      "component": "api",
      "tier": "backend"
    },
    "EnableRuntimeInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true
  }
}
```

### Configuração por Ambiente

#### Desenvolvimento

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao-Dev",
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Debug",
    "AdditionalLabels": {
      "environment": "development"
    }
  }
}
```

#### Produção

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "EnableConsoleLogging": false,
    "MinimumLogLevel": "Information",
    "LokiUrl": "http://loki.monitoring.svc.cluster.local:3100",
    "CollectorEndpoint": "http://otel-collector.monitoring.svc.cluster.local:4317",
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0"
    }
  }
}
```

## 🔧 Configuração via Código

### Configuração Básica

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
});
```

### Configuração Avançada

```csharp
builder.Services.AddObservability(options =>
{
    // Identificação do serviço
    options.ServiceName = "MinhaAplicacao";
    
    // Configuração de métricas
    options.PrometheusPort = 9090;
    options.EnableMetrics = true;
    options.EnableRuntimeInstrumentation = true;
    options.EnableAspNetCoreInstrumentation = true;
    options.EnableHttpClientInstrumentation = true;
    
    // Configuração de traces
    options.EnableTracing = true;
    options.OtlpEndpoint = "http://localhost:4317";
    
    // Configuração de logs
    options.EnableLogging = true;
    options.EnableConsoleLogging = true;
    options.MinimumLogLevel = "Information";
    options.LokiUrl = "http://localhost:3100";
    options.EnableCorrelationId = true;
    
    // Labels customizados
    options.AdditionalLabels.Add("environment", "production");
    options.AdditionalLabels.Add("version", "1.0.0");
    options.AdditionalLabels.Add("team", "backend");
    
    options.LokiLabels.Add("app", "minha-aplicacao");
    options.LokiLabels.Add("component", "api");
});
```

## 🏷️ Labels e Tags

### AdditionalLabels

Labels adicionais são aplicados a todas as métricas:

```csharp
options.AdditionalLabels.Add("environment", "production");
options.AdditionalLabels.Add("version", "1.0.0");
options.AdditionalLabels.Add("team", "backend");
options.AdditionalLabels.Add("region", "us-east-1");
```

### LokiLabels

Labels customizados para categorização de logs no Loki:

```csharp
options.LokiLabels.Add("app", "minha-aplicacao");
options.LokiLabels.Add("component", "api");
options.LokiLabels.Add("tier", "backend");
options.LokiLabels.Add("service", "user-management");
```

## 📊 Configuração de Métricas

### Habilitar/Desabilitar Instrumentação

```csharp
// Apenas métricas básicas
options.EnableRuntimeInstrumentation = false;
options.EnableAspNetCoreInstrumentation = false;
options.EnableHttpClientInstrumentation = false;

// Apenas métricas de runtime .NET
options.EnableRuntimeInstrumentation = true;
options.EnableAspNetCoreInstrumentation = false;
options.EnableHttpClientInstrumentation = false;
```

### Porta do Prometheus

```csharp
// Porta customizada
options.PrometheusPort = 9110;

// Mapear endpoint customizado
app.MapPrometheusScrapingEndpoint("/custom-metrics");
```

## 📝 Configuração de Logs

### Níveis de Log

```csharp
// Níveis disponíveis: Debug, Information, Warning, Error, Fatal
options.MinimumLogLevel = "Debug";  // Mais verboso
options.MinimumLogLevel = "Information";  // Padrão
options.MinimumLogLevel = "Warning";  // Menos verboso
```

### Console vs Loki

```csharp
// Apenas console (desenvolvimento)
options.EnableConsoleLogging = true;
options.LokiUrl = null;

// Apenas Loki (produção)
options.EnableConsoleLogging = false;
options.LokiUrl = "http://loki:3100";

// Ambos
options.EnableConsoleLogging = true;
options.LokiUrl = "http://loki:3100";
```

### Correlation ID

```csharp
// Habilitar correlation ID automático
options.EnableCorrelationId = true;

// Desabilitar (se gerenciado externamente)
options.EnableCorrelationId = false;
```

## 🔍 Configuração de Traces

### Endpoint OTLP

```csharp
// Tempo
options.OtlpEndpoint = "http://localhost:4317";

// Zipkin
options.OtlpEndpoint = "http://localhost:9411/api/v2/spans";

// Custom collector
options.OtlpEndpoint = "http://my-collector:4317";
```

### Instrumentação Seletiva

```csharp
// Apenas ASP.NET Core
options.EnableAspNetCoreInstrumentation = true;
options.EnableHttpClientInstrumentation = false;

// Apenas HTTP Client
options.EnableAspNetCoreInstrumentation = false;
options.EnableHttpClientInstrumentation = true;
```

## 🌍 Configuração por Ambiente

### Desenvolvimento

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MinhaAplicacao-Dev";
        options.EnableConsoleLogging = true;
        options.MinimumLogLevel = "Debug";
        options.AdditionalLabels.Add("environment", "development");
    });
}
```

### Produção

```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MinhaAplicacao";
        options.EnableConsoleLogging = false;
        options.MinimumLogLevel = "Information";
        options.LokiUrl = "http://loki.monitoring.svc.cluster.local:3100";
        options.CollectorEndpoint = "http://otel-collector.monitoring.svc.cluster.local:4317";
        options.AdditionalLabels.Add("environment", "production");
    });
}
```

## 🔧 Configuração Avançada

### Múltiplas Configurações

```csharp
// Configuração base
builder.Services.AddObservability(builder.Configuration);

// Configuração adicional via código
builder.Services.Configure<ObservabilityOptions>(options =>
{
    options.AdditionalLabels.Add("deployment", "v1.0.0");
    options.LokiLabels.Add("deployment", "v1.0.0");
});
```

### Configuração Condicional

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    
    // Habilitar apenas métricas em produção
    if (builder.Environment.IsProduction())
    {
        options.EnableMetrics = true;
        options.EnableTracing = true;
        options.EnableLogging = true;
    }
    else
    {
        options.EnableMetrics = true;
        options.EnableTracing = false;
        options.EnableLogging = true;
    }
});
```

## ❓ Troubleshooting

### Problemas Comuns

1. **URLs inválidas**: Verifique se as URLs do Loki e OTLP estão corretas
2. **Porta em uso**: Verifique se a porta do Prometheus está disponível
3. **Dependências faltando**: Verifique se todas as dependências estão instaladas
4. **Configuração incorreta**: Verifique se a seção "Observability" está no appsettings.json

### Validação de Configuração

```csharp
// Validar configuração
var options = builder.Configuration.GetSection("Observability").Get<ObservabilityOptions>();
if (options == null)
{
    throw new InvalidOperationException("Configuração de Observability não encontrada");
}

if (string.IsNullOrEmpty(options.ServiceName))
{
    throw new InvalidOperationException("ServiceName é obrigatório");
}
```

## 📚 Recursos Adicionais

- [Guia de Início Rápido](getting-started.md)
- [Exemplos](examples.md)
- [API Reference](api-reference.md)
- [Troubleshooting](troubleshooting.md)
