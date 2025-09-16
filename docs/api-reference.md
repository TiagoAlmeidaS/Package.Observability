# API Reference

Documentação completa da API do Package.Observability.

## 📚 Namespaces

- `Package.Observability` - Classes principais do pacote

## 🔧 Classes Principais

### ObservabilityStartupExtensions

Classe estática com métodos de extensão para configurar observabilidade.

#### Métodos

##### AddObservability(IConfiguration, string)

```csharp
public static IServiceCollection AddObservability(
    this IServiceCollection services, 
    IConfiguration configuration, 
    string sectionName = "Observability")
```

**Parâmetros:**
- `services`: Coleção de serviços
- `configuration`: Configuração da aplicação
- `sectionName`: Nome da seção de configuração (padrão: "Observability")

**Retorno:** `IServiceCollection` para encadeamento

**Exemplo:**
```csharp
builder.Services.AddObservability(builder.Configuration);
```

##### AddObservability(Action<ObservabilityOptions>)

```csharp
public static IServiceCollection AddObservability(
    this IServiceCollection services,
    Action<ObservabilityOptions> configureOptions)
```

**Parâmetros:**
- `services`: Coleção de serviços
- `configureOptions`: Ação para configurar opções

**Retorno:** `IServiceCollection` para encadeamento

**Exemplo:**
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    options.EnableMetrics = true;
});
```

### ObservabilityOptions

Classe de configuração para opções de observabilidade.

#### Propriedades

##### ServiceName

```csharp
public string ServiceName { get; set; } = "DefaultService";
```

Nome do serviço para identificação em métricas, logs e traces.

##### PrometheusPort

```csharp
public int PrometheusPort { get; set; } = 9090;
```

Porta do endpoint de métricas Prometheus.

##### EnableMetrics

```csharp
public bool EnableMetrics { get; set; } = true;
```

Habilita coleta de métricas.

##### EnableTracing

```csharp
public bool EnableTracing { get; set; } = true;
```

Habilita rastreamento distribuído.

##### EnableLogging

```csharp
public bool EnableLogging { get; set; } = true;
```

Habilita logs estruturados.

##### LokiUrl

```csharp
public string LokiUrl { get; set; } = "http://localhost:3100";
```

URL do Grafana Loki para logs.

##### OtlpEndpoint

```csharp
public string OtlpEndpoint { get; set; } = "http://localhost:4317";
```

Endpoint OTLP para traces.

##### EnableConsoleLogging

```csharp
public bool EnableConsoleLogging { get; set; } = true;
```

Habilita logs no console.

##### MinimumLogLevel

```csharp
public string MinimumLogLevel { get; set; } = "Information";
```

Nível mínimo de log (Debug, Information, Warning, Error, Fatal).

##### AdditionalLabels

```csharp
public Dictionary<string, string> AdditionalLabels { get; set; } = new();
```

Labels adicionais para métricas.

##### LokiLabels

```csharp
public Dictionary<string, string> LokiLabels { get; set; } = new();
```

Labels customizados para Loki.

##### EnableCorrelationId

```csharp
public bool EnableCorrelationId { get; set; } = true;
```

Habilita Correlation ID automático.

##### EnableRuntimeInstrumentation

```csharp
public bool EnableRuntimeInstrumentation { get; set; } = true;
```

Habilita métricas de runtime .NET.

##### EnableHttpClientInstrumentation

```csharp
public bool EnableHttpClientInstrumentation { get; set; } = true;
```

Habilita instrumentação HTTP Client.

##### EnableAspNetCoreInstrumentation

```csharp
public bool EnableAspNetCoreInstrumentation { get; set; } = true;
```

Habilita instrumentação ASP.NET Core.

### ObservabilityMetrics

Classe estática para criação e gerenciamento de métricas customizadas.

#### Métodos

##### GetOrCreateMeter(string, string?)

```csharp
public static Meter GetOrCreateMeter(string serviceName, string? version = null)
```

Obtém ou cria um Meter para o nome do serviço especificado.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `version`: Versão do serviço (opcional, padrão: "1.0.0")

**Retorno:** `Meter` - Instância do Meter

**Exceções:**
- `ArgumentException`: Se serviceName for null ou vazio

**Exemplo:**
```csharp
var meter = ObservabilityMetrics.GetOrCreateMeter("MinhaAplicacao", "1.0.0");
```

##### CreateCounter<T>(string, string, string?, string?)

```csharp
public static Counter<T> CreateCounter<T>(
    string serviceName,
    string name,
    string? unit = null,
    string? description = null) where T : struct
```

Cria um contador de métricas.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `name`: Nome da métrica
- `unit`: Unidade de medida (opcional)
- `description`: Descrição da métrica (opcional)

**Retorno:** `Counter<T>` - Instância do contador

**Exemplo:**
```csharp
var counter = ObservabilityMetrics.CreateCounter<int>(
    "MinhaAplicacao", 
    "requests_total", 
    "count", 
    "Total de requisições");
```

##### CreateHistogram<T>(string, string, string?, string?)

```csharp
public static Histogram<T> CreateHistogram<T>(
    string serviceName,
    string name,
    string? unit = null,
    string? description = null) where T : struct
```

Cria um histograma de métricas.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `name`: Nome da métrica
- `unit`: Unidade de medida (opcional)
- `description`: Descrição da métrica (opcional)

**Retorno:** `Histogram<T>` - Instância do histograma

**Exemplo:**
```csharp
var histogram = ObservabilityMetrics.CreateHistogram<double>(
    "MinhaAplicacao", 
    "request_duration", 
    "ms", 
    "Duração das requisições");
```

##### CreateObservableGauge<T>(string, string, Func<T>, string?, string?)

```csharp
public static ObservableGauge<T> CreateObservableGauge<T>(
    string serviceName,
    string name,
    Func<T> observeValue,
    string? unit = null,
    string? description = null) where T : struct
```

Cria um gauge observável.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `name`: Nome da métrica
- `observeValue`: Função para obter o valor atual
- `unit`: Unidade de medida (opcional)
- `description`: Descrição da métrica (opcional)

**Retorno:** `ObservableGauge<T>` - Instância do gauge

**Exemplo:**
```csharp
var gauge = ObservabilityMetrics.CreateObservableGauge<int>(
    "MinhaAplicacao", 
    "active_connections", 
    () => GetActiveConnections(), 
    "connections", 
    "Conexões ativas");
```

##### CreateUpDownCounter<T>(string, string, string?, string?)

```csharp
public static UpDownCounter<T> CreateUpDownCounter<T>(
    string serviceName,
    string name,
    string? unit = null,
    string? description = null) where T : struct
```

Cria um contador up-down.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `name`: Nome da métrica
- `unit`: Unidade de medida (opcional)
- `description`: Descrição da métrica (opcional)

**Retorno:** `UpDownCounter<T>` - Instância do contador

**Exemplo:**
```csharp
var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>(
    "MinhaAplicacao", 
    "queue_size", 
    "items", 
    "Tamanho da fila");
```

##### DisposeAll()

```csharp
public static void DisposeAll()
```

Descarta todas as instâncias de Meter criadas.

**Exemplo:**
```csharp
// No final da aplicação
ObservabilityMetrics.DisposeAll();
```

### ActivitySourceFactory

Classe estática para criação e gerenciamento de ActivitySource para traces.

#### Métodos

##### GetOrCreate(string, string?)

```csharp
public static ActivitySource GetOrCreate(string serviceName, string? version = null)
```

Obtém ou cria um ActivitySource para o nome do serviço especificado.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `version`: Versão do serviço (opcional, padrão: "1.0.0")

**Retorno:** `ActivitySource` - Instância do ActivitySource

**Exceções:**
- `ArgumentException`: Se serviceName for null ou vazio

**Exemplo:**
```csharp
var activitySource = ActivitySourceFactory.GetOrCreate("MinhaAplicacao", "1.0.0");
```

##### StartActivity(string, string, ActivityKind, ActivityContext)

```csharp
public static Activity? StartActivity(
    string serviceName, 
    string activityName, 
    ActivityKind kind = ActivityKind.Internal,
    ActivityContext parentContext = default)
```

Cria uma nova atividade com o nome especificado.

**Parâmetros:**
- `serviceName`: Nome do serviço
- `activityName`: Nome da atividade
- `kind`: Tipo da atividade (padrão: Internal)
- `parentContext`: Contexto da atividade pai (opcional)

**Retorno:** `Activity?` - Instância da atividade ou null se não for amostrada

**Exemplo:**
```csharp
using var activity = ActivitySourceFactory.StartActivity(
    "MinhaAplicacao", 
    "ProcessarRequisicao", 
    ActivityKind.Server);
```

##### DisposeAll()

```csharp
public static void DisposeAll()
```

Descarta todas as instâncias de ActivitySource criadas.

**Exemplo:**
```csharp
// No final da aplicação
ActivitySourceFactory.DisposeAll();
```

## 🔧 Tipos de Métricas

### Counter<T>

Contador que só aumenta.

**Métodos:**
- `Add(T value, params KeyValuePair<string, object?>[] tags)`

**Exemplo:**
```csharp
var counter = ObservabilityMetrics.CreateCounter<int>("MinhaAplicacao", "requests_total");
counter.Add(1, new KeyValuePair<string, object?>("status", "success"));
```

### Histogram<T>

Histograma para medir distribuições de valores.

**Métodos:**
- `Record(T value, params KeyValuePair<string, object?>[] tags)`

**Exemplo:**
```csharp
var histogram = ObservabilityMetrics.CreateHistogram<double>("MinhaAplicacao", "request_duration");
histogram.Record(150.5, new KeyValuePair<string, object?>("endpoint", "/api/users"));
```

### ObservableGauge<T>

Gauge observável para valores que mudam ao longo do tempo.

**Exemplo:**
```csharp
var gauge = ObservabilityMetrics.CreateObservableGauge<int>(
    "MinhaAplicacao", 
    "active_connections", 
    () => GetActiveConnections());
```

### UpDownCounter<T>

Contador que pode aumentar ou diminuir.

**Métodos:**
- `Add(T value, params KeyValuePair<string, object?>[] tags)`

**Exemplo:**
```csharp
var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>("MinhaAplicacao", "queue_size");
upDownCounter.Add(5);  // Aumenta
upDownCounter.Add(-2); // Diminui
```

## 🔍 Tipos de Atividades

### ActivityKind

- `Internal`: Atividade interna
- `Server`: Atividade de servidor (recebe requisição)
- `Client`: Atividade de cliente (faz requisição)
- `Producer`: Atividade de produtor
- `Consumer`: Atividade de consumidor

### Exemplo de Uso

```csharp
// Atividade de servidor
using var serverActivity = ActivitySourceFactory.StartActivity(
    "MinhaAplicacao", 
    "HandleRequest", 
    ActivityKind.Server);

// Atividade de cliente
using var clientActivity = ActivitySourceFactory.StartActivity(
    "MinhaAplicacao", 
    "CallExternalApi", 
    ActivityKind.Client);

// Atividade interna
using var internalActivity = ActivitySourceFactory.StartActivity(
    "MinhaAplicacao", 
    "ProcessData", 
    ActivityKind.Internal);
```

## 📊 Métricas Automáticas

### Runtime .NET

- `process_runtime_dotnet_gc_heap_size_bytes`
- `process_runtime_dotnet_gc_collections_total`
- `process_runtime_dotnet_thread_pool_threads_count`
- `process_runtime_dotnet_exceptions_total`

### ASP.NET Core

- `http_requests_received_total`
- `http_requests_duration_seconds`
- `http_requests_active`

### HTTP Client

- `http_client_requests_total`
- `http_client_requests_duration_seconds`

## 🔧 Configuração de Instrumentação

### Habilitar/Desabilitar Instrumentação

```csharp
builder.Services.AddObservability(options =>
{
    // Apenas métricas de runtime
    options.EnableRuntimeInstrumentation = true;
    options.EnableAspNetCoreInstrumentation = false;
    options.EnableHttpClientInstrumentation = false;
    
    // Apenas traces de ASP.NET Core
    options.EnableAspNetCoreInstrumentation = true;
    options.EnableHttpClientInstrumentation = false;
});
```

## 📚 Recursos Adicionais

- [Guia de Início Rápido](getting-started.md)
- [Configuração](configuration.md)
- [Exemplos](examples.md)
- [Troubleshooting](troubleshooting.md)
