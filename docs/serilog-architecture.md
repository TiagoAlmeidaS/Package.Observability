# Arquitetura do SerilogService - Package.Observability

## Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                    Package.Observability                       │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Metrics       │  │   Tracing       │  │   Logging       │  │
│  │   (Prometheus)  │  │ (OpenTelemetry) │  │   (Serilog)     │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    SerilogService                              │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Console       │  │     File        │  │     Loki        │  │
│  │     Sink        │  │     Sink        │  │     Sink        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │     Seq         │  │ Elasticsearch   │  │   Custom        │  │
│  │     Sink        │  │     Sink        │  │   Sinks         │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Health Checks                               │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ Observability   │  │   Logging       │  │   Serilog       │  │
│  │ Health Check    │  │ Health Check    │  │ Health Check    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Fluxo de Configuração

```
1. Program.cs
   │
   ▼
2. AddObservability()
   │
   ▼
3. ObservabilityOptions (Configuration)
   │
   ▼
4. SerilogService (Singleton)
   │
   ▼
5. SerilogServiceInitializer (HostedService)
   │
   ▼
6. Configure() → LoggerConfiguration
   │
   ▼
7. Multiple Sinks (Console, File, Loki, Seq, etc.)
```

## Integração com ASP.NET Core

```
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Application                    │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Controllers   │  │   Services      │  │   Middleware    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                │                                │
│                                ▼                                │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Dependency Injection Container                 │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │ │
│  │  │ ILogger<T>      │  │ SerilogService  │  │ Other       │  │ │
│  │  │ (Default)       │  │ (Custom)        │  │ Services    │  │ │
│  │  └─────────────────┘  └─────────────────┘  └─────────────┘  │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Logging Outputs                             │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Console       │  │   Files         │  │   Loki/Grafana  │  │
│  │   (Development) │  │   (Audit)       │  │   (Production)  │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │     Seq         │  │ Elasticsearch   │  │   Custom        │  │
│  │   (Analysis)    │  │   (Search)      │  │   Destinations  │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Configuração por Ambiente

### Development
```json
{
  "Observability": {
    "EnableConsoleLogging": true,
    "EnableFileLogging": true,
    "MinimumLogLevel": "Debug"
  }
}
```

### Production
```json
{
  "Observability": {
    "EnableConsoleLogging": false,
    "EnableFileLogging": true,
    "EnableSeqLogging": true,
    "LokiUrl": "http://loki:3100",
    "MinimumLogLevel": "Information"
  }
}
```

## Padrões de Uso

### 1. Logging Básico
```csharp
// Usando ILogger padrão
_logger.LogInformation("Mensagem simples");

// Usando SerilogService
_serilogService.Log(LogEventLevel.Information, 
    "Mensagem estruturada com {Property}", value);
```

### 2. Logging com Contexto
```csharp
// Logger com contexto específico
var scopedLogger = _serilogService.CreateScopedLogger(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["RequestId"] = requestId
});

scopedLogger.LogInformation("Operação executada");
```

### 3. Health Checks
```csharp
// Verificar status do SerilogService
var status = _serilogService.GetConfigurationStatus();
if (!status.IsConfigured)
{
    // Handle configuration issues
}
```

## Benefícios da Integração

1. **Consistência**: Segue o mesmo padrão dos outros serviços de telemetria
2. **Flexibilidade**: Configuração via ObservabilityOptions
3. **Monitoramento**: Health checks específicos para Serilog
4. **Extensibilidade**: Fácil adição de novos sinks
5. **Manutenibilidade**: Código organizado e testável
