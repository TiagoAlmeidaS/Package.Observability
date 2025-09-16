# ⚙️ Exemplos de Configuração - Package.Observability

Este documento contém exemplos práticos de configuração do `Package.Observability` para diferentes cenários e necessidades.

## 🎯 Cenários de Uso

### 1. Desenvolvimento Local (Sem Infraestrutura Externa)

**Objetivo**: Desenvolvimento local sem necessidade de Prometheus, Loki ou Jaeger.

```json
// appsettings.Development.json
{
  "Observability": {
    "ServiceName": "MeuServico-Dev",
    "EnableMetrics": true,           // Apenas métricas locais
    "EnableTracing": false,          // Sem tracing
    "EnableLogging": true,           // Logs no console
    "EnableConsoleLogging": true,    // Console habilitado
    "LokiUrl": "",                  // Sem Loki
    "OtlpEndpoint": "",             // Sem OTLP
    "MinimumLogLevel": "Debug",     // Logs detalhados
    "AdditionalLabels": {
      "environment": "development",
      "debug": "true"
    }
  }
}
```

**Código**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability(builder.Configuration);
var app = builder.Build();
app.Run();
```

**Resultado**:
- ✅ Métricas disponíveis em `http://localhost:9090/metrics`
- ✅ Logs no console com formato estruturado
- ❌ Sem tracing distribuído
- ❌ Sem agregação de logs

### 2. Apenas Logs (Sem Métricas e Tracing)

**Objetivo**: Apenas logging estruturado, sem métricas ou tracing.

```json
// appsettings.json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": false,         // Sem métricas
    "EnableTracing": false,         // Sem tracing
    "EnableLogging": true,          // Apenas logs
    "EnableConsoleLogging": true,   // Console habilitado
    "LokiUrl": "",                 // Sem Loki (apenas console)
    "OtlpEndpoint": "",            // Sem OTLP
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true
  }
}
```

**Código**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddObservability(builder.Configuration);
var app = builder.Build();
app.Run();
```

**Resultado**:
- ❌ Sem métricas
- ✅ Logs estruturados no console
- ❌ Sem tracing distribuído
- ✅ Correlation ID automático

### 3. Apenas Métricas (Sem Logs e Tracing)

**Objetivo**: Apenas coleta de métricas para monitoramento.

```json
// appsettings.json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "PrometheusPort": 9090,
    "EnableMetrics": true,          // Apenas métricas
    "EnableTracing": false,         // Sem tracing
    "EnableLogging": false,         // Sem logs
    "EnableConsoleLogging": false,  // Sem console
    "LokiUrl": "",                 // Sem Loki
    "OtlpEndpoint": "",            // Sem OTLP
    "EnableRuntimeInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true,
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0"
    }
  }
}
```

**Resultado**:
- ✅ Métricas completas em `http://localhost:9090/metrics`
- ❌ Sem logs
- ❌ Sem tracing distribuído
- ✅ Instrumentação automática de runtime, HTTP e ASP.NET Core

### 4. Apenas Tracing (Sem Métricas e Logs)

**Objetivo**: Apenas rastreamento distribuído para debugging.

```json
// appsettings.json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": false,         // Sem métricas
    "EnableTracing": true,          // Apenas tracing
    "EnableLogging": false,         // Sem logs
    "EnableConsoleLogging": false,  // Sem console
    "LokiUrl": "",                 // Sem Loki
    "OtlpEndpoint": "http://jaeger:4317",  // Apenas OTLP
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true
  }
}
```

**Resultado**:
- ❌ Sem métricas
- ❌ Sem logs
- ✅ Traces enviados para Jaeger
- ✅ Instrumentação automática de HTTP e ASP.NET Core

### 5. Produção Completa (Métricas + Logs + Tracing)

**Objetivo**: Observabilidade completa para produção.

```json
// appsettings.Production.json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "PrometheusPort": 9090,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "EnableConsoleLogging": false,  // Sem console em produção
    "LokiUrl": "http://loki.monitoring.svc.cluster.local:3100",
    "OtlpEndpoint": "http://jaeger.monitoring.svc.cluster.local:4317",
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    "EnableRuntimeInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true,
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0",
      "team": "backend",
      "region": "us-east-1"
    },
    "LokiLabels": {
      "app": "meu-servico",
      "component": "api",
      "tier": "backend"
    }
  }
}
```

**Resultado**:
- ✅ Métricas completas em `http://localhost:9090/metrics`
- ✅ Logs estruturados enviados para Loki
- ✅ Traces enviados para Jaeger
- ✅ Instrumentação automática completa
- ✅ Correlation ID automático

## 🔧 Configurações por Ambiente

### Desenvolvimento

```json
// appsettings.Development.json
{
  "Observability": {
    "ServiceName": "MeuServico-Dev",
    "EnableMetrics": true,
    "EnableTracing": false,
    "EnableLogging": true,
    "EnableConsoleLogging": true,
    "LokiUrl": "",
    "OtlpEndpoint": "",
    "MinimumLogLevel": "Debug",
    "AdditionalLabels": {
      "environment": "development",
      "debug": "true"
    }
  }
}
```

### Staging

```json
// appsettings.Staging.json
{
  "Observability": {
    "ServiceName": "MeuServico-Staging",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "EnableConsoleLogging": false,
    "LokiUrl": "http://loki-staging:3100",
    "OtlpEndpoint": "http://jaeger-staging:4317",
    "MinimumLogLevel": "Information",
    "AdditionalLabels": {
      "environment": "staging",
      "version": "1.0.0"
    }
  }
}
```

### Produção

```json
// appsettings.Production.json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "EnableConsoleLogging": false,
    "LokiUrl": "http://loki.monitoring.svc.cluster.local:3100",
    "OtlpEndpoint": "http://jaeger.monitoring.svc.cluster.local:4317",
    "MinimumLogLevel": "Warning",
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0",
      "team": "backend"
    },
    "LokiLabels": {
      "app": "meu-servico",
      "component": "api"
    }
  }
}
```

## 🐳 Configurações Docker

### Docker Compose para Desenvolvimento

```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  meu-servico:
    build: .
    ports:
      - "5000:80"
      - "9090:9090"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Observability__ServiceName=MeuServico-Dev
      - Observability__EnableMetrics=true
      - Observability__EnableTracing=false
      - Observability__EnableLogging=true
      - Observability__EnableConsoleLogging=true
      - Observability__LokiUrl=
      - Observability__OtlpEndpoint=
    depends_on:
      - prometheus
      - grafana

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9091:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana

volumes:
  grafana-storage:
```

### Docker Compose para Produção

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  meu-servico:
    build: .
    ports:
      - "80:80"
      - "9090:9090"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Observability__ServiceName=MeuServico
      - Observability__EnableMetrics=true
      - Observability__EnableTracing=true
      - Observability__EnableLogging=true
      - Observability__EnableConsoleLogging=false
      - Observability__LokiUrl=http://loki:3100
      - Observability__OtlpEndpoint=http://jaeger:4317
      - Observability__MinimumLogLevel=Information
    depends_on:
      - loki
      - jaeger
      - prometheus

  loki:
    image: grafana/loki:latest
    ports:
      - "3100:3100"
    command: -config.file=/etc/loki/local-config.yaml

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "4317:4317"
    environment:
      - COLLECTOR_OTLP_ENABLED=true

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9091:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana

volumes:
  grafana-storage:
```

## 🔧 Configurações Avançadas

### 1. Configuração com Validação Customizada

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

try
{
    // Tentar configuração completa
    builder.Services.AddObservability(builder.Configuration);
}
catch (ObservabilityConfigurationException ex)
{
    // Log do erro
    Console.WriteLine($"Erro de configuração: {ex.Message}");
    
    // Fallback para configuração mínima
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MeuServico-Fallback";
        options.EnableMetrics = false;
        options.EnableTracing = false;
        options.EnableLogging = true;
        options.EnableConsoleLogging = true;
    });
}

var app = builder.Build();
app.Run();
```

### 2. Configuração Baseada em Variáveis de Ambiente

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuração baseada em variáveis de ambiente
var environment = builder.Environment.EnvironmentName;
var enableMetrics = Environment.GetEnvironmentVariable("ENABLE_METRICS") == "true";
var enableTracing = Environment.GetEnvironmentVariable("ENABLE_TRACING") == "true";
var enableLogging = Environment.GetEnvironmentVariable("ENABLE_LOGGING") == "true";
var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL") ?? "";
var otlpEndpoint = Environment.GetEnvironmentVariable("OTLP_ENDPOINT") ?? "";

builder.Services.AddObservability(options =>
{
    options.ServiceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "MeuServico";
    options.EnableMetrics = enableMetrics;
    options.EnableTracing = enableTracing;
    options.EnableLogging = enableLogging;
    options.EnableConsoleLogging = environment == "Development";
    options.LokiUrl = lokiUrl;
    options.OtlpEndpoint = otlpEndpoint;
    options.MinimumLogLevel = environment == "Production" ? "Warning" : "Information";
    
    // Labels baseados no ambiente
    options.AdditionalLabels.Add("environment", environment);
    options.AdditionalLabels.Add("version", Environment.GetEnvironmentVariable("VERSION") ?? "1.0.0");
});

var app = builder.Build();
app.Run();
```

### 3. Configuração com Health Checks Customizados

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(builder.Configuration);

// Health checks customizados
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy("Database OK"))
    .AddCheck("external-api", () => HealthCheckResult.Healthy("External API OK"))
    .AddCheck("observability", () => 
    {
        // Verificar se os serviços de observabilidade estão funcionando
        try
        {
            // Sua lógica de verificação aqui
            return HealthCheckResult.Healthy("Observabilidade OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded($"Observabilidade com problemas: {ex.Message}");
        }
    });

var app = builder.Build();

// Mapear health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.Run();
```

## 📊 Configurações de Monitoramento

### 1. Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "rules/*.yml"

scrape_configs:
  - job_name: 'meu-servico'
    static_configs:
      - targets: ['meu-servico:9090']
    scrape_interval: 5s
    metrics_path: /metrics
    
  - job_name: 'meu-servico-staging'
    static_configs:
      - targets: ['meu-servico-staging:9090']
    scrape_interval: 10s
    metrics_path: /metrics

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093
```

### 2. Grafana Dashboard Configuration

```json
{
  "dashboard": {
    "title": "MeuServico - Observabilidade",
    "panels": [
      {
        "title": "HTTP Requests",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{status}}"
          }
        ]
      },
      {
        "title": "Request Duration",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
```

## 🚨 Configurações de Alertas

### 1. Prometheus Alert Rules

```yaml
# rules/meu-servico.yml
groups:
  - name: meu-servico
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value }} errors per second"
          
      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "95th percentile response time is {{ $value }} seconds"
          
      - alert: ServiceDown
        expr: up{job="meu-servico"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service is down"
          description: "MeuServico is not responding"
```

## 🔍 Configurações de Debug

### 1. Debug de Configuração

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Debug de configuração
var configuration = builder.Configuration.GetSection("Observability");
var options = configuration.Get<ObservabilityOptions>();

Console.WriteLine("=== Configuração de Observabilidade ===");
Console.WriteLine($"ServiceName: {options.ServiceName}");
Console.WriteLine($"EnableMetrics: {options.EnableMetrics}");
Console.WriteLine($"EnableTracing: {options.EnableTracing}");
Console.WriteLine($"EnableLogging: {options.EnableLogging}");
Console.WriteLine($"EnableConsoleLogging: {options.EnableConsoleLogging}");
Console.WriteLine($"LokiUrl: {options.LokiUrl}");
Console.WriteLine($"OtlpEndpoint: {options.OtlpEndpoint}");
Console.WriteLine($"MinimumLogLevel: {options.MinimumLogLevel}");
Console.WriteLine($"PrometheusPort: {options.PrometheusPort}");
Console.WriteLine("=======================================");

builder.Services.AddObservability(builder.Configuration);
var app = builder.Build();
app.Run();
```

### 2. Log de Configuração

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configurar logging antes de AddObservability
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

try
{
    builder.Services.AddObservability(builder.Configuration);
    logger.LogInformation("Observabilidade configurada com sucesso");
}
catch (ObservabilityConfigurationException ex)
{
    logger.LogError(ex, "Erro ao configurar observabilidade: {Message}", ex.Message);
    
    // Fallback
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MeuServico-Fallback";
        options.EnableMetrics = false;
        options.EnableTracing = false;
        options.EnableLogging = true;
        options.EnableConsoleLogging = true;
    });
    
    logger.LogWarning("Usando configuração de fallback");
}

var app = builder.Build();
app.Run();
```

## 📚 Exemplos de Uso por Tipo de Aplicação

### 1. ASP.NET Core Web API

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração de observabilidade
builder.Services.AddObservability(builder.Configuration);

// Outros serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuração do pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### 2. Worker Service

```csharp
// Program.cs
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

// Configuração de observabilidade
builder.Services.AddObservability(builder.Configuration);

// Worker service
builder.Services.AddHostedService<MeuWorker>();

var host = builder.Build();
host.Run();

// Worker.cs
public class MeuWorker : BackgroundService
{
    private readonly ILogger<MeuWorker> _logger;
    
    public MeuWorker(ILogger<MeuWorker> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker executando em: {Time}", DateTimeOffset.Now);
            
            // Seu código aqui
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

### 3. Console Application

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

// Configuração de observabilidade
builder.Services.AddObservability(builder.Configuration);

// Serviços customizados
builder.Services.AddTransient<MeuServico>();

var host = builder.Build();

// Executar aplicação
var servico = host.Services.GetRequiredService<MeuServico>();
await servico.ExecutarAsync();

await host.RunAsync();
```

---

Estes exemplos cobrem todos os cenários de uso do `Package.Observability`. Escolha a configuração que melhor se adapta ao seu ambiente e necessidades.
