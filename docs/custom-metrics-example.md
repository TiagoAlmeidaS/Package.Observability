# Exemplo de Configuração de Métricas Personalizadas

Este documento mostra como configurar métricas personalizadas por rota, similar ao exemplo fornecido.

## Configuração Básica

```csharp
// Program.cs
builder.Services.AddObservability(builder.Configuration);

// Adicionar middleware de métricas personalizadas
app.UseCustomRouteMetrics();
```

## Configuração Avançada

```csharp
// Program.cs
builder.Services.AddObservability(options =>
{
    options.ServiceName = "testeLoki3";
    options.ServiceVersion = "1.0.0";
    
    // Configurações de Tracing
    options.EnableTracing = true;
    options.OtlpEndpoint = "http://localhost:4317";
    options.OtlpProtocol = "Grpc";
    options.RecordExceptions = true;
    options.ExcludePaths = new List<string> { "/metrics", "/health" };
    
    // Configurações de Métricas Personalizadas
    options.EnableRouteMetrics = true;
    options.EnableDetailedEndpointMetrics = true;
    
    // Buckets personalizados para histograma
    options.CustomHistogramBuckets = new List<double> { 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 };
    
    // Labels customizados para métricas
    options.CustomMetricLabels = new Dictionary<string, string>
    {
        { "environment", "production" },
        { "region", "us-east-1" }
    };
    
    // Nomes personalizados para métricas
    options.MetricNames = new MetricNamesConfiguration
    {
        HttpRequestsTotal = "http_requests_total_by_route",
        HttpRequestErrorsTotal = "http_requests_errors_total_by_route",
        HttpRequestDurationSeconds = "http_request_duration_seconds_by_route"
    };
});

// Adicionar middleware de métricas personalizadas
app.UseCustomRouteMetrics();
```

## Configuração via appsettings.json

```json
{
  "Observability": {
    "ServiceName": "testeLoki3",
    "ServiceVersion": "1.0.0",
    "EnableTracing": true,
    "EnableMetrics": true,
    "OtlpEndpoint": "http://localhost:4317",
    "OtlpProtocol": "Grpc",
    "RecordExceptions": true,
    "ExcludePaths": ["/metrics", "/health"],
    "EnableRouteMetrics": true,
    "EnableDetailedEndpointMetrics": true,
    "CustomHistogramBuckets": [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10],
    "CustomMetricLabels": {
      "environment": "production",
      "region": "us-east-1"
    },
    "MetricNames": {
      "HttpRequestsTotal": "http_requests_total_by_route",
      "HttpRequestErrorsTotal": "http_requests_errors_total_by_route",
      "HttpRequestDurationSeconds": "http_request_duration_seconds_by_route"
    }
  }
}
```

## Métricas Geradas

O middleware personalizado gera as seguintes métricas:

### 1. Contador de Requisições HTTP
- **Nome**: `http_requests_total_by_route` (configurável)
- **Labels**: `method`, `endpoint`, `route`
- **Descrição**: Total de requisições HTTP, rotuladas por método, nome do endpoint e template da rota

### 2. Contador de Erros HTTP
- **Nome**: `http_requests_errors_total_by_route` (configurável)
- **Labels**: `method`, `endpoint`, `route`
- **Descrição**: Total de erros de requisição HTTP (5xx ou exceções não tratadas)

### 3. Histograma de Duração de Requisições
- **Nome**: `http_request_duration_seconds_by_route` (configurável)
- **Labels**: `method`, `endpoint`, `route`
- **Descrição**: Duração das requisições HTTP em segundos
- **Buckets**: Configuráveis (padrão: buckets exponenciais de 5ms a ~163s)

## Comparação com o Exemplo Original

A implementação agora suporta todas as funcionalidades do exemplo fornecido:

✅ **OpenTelemetry Tracing com OTLP gRPC**
✅ **Configuração de Resource com ServiceName e ServiceVersion**
✅ **AspNetCoreInstrumentation com RecordException e Filter**
✅ **Métricas personalizadas por rota com labels específicos**
✅ **Buckets exponenciais configuráveis**
✅ **Filtros para evitar ruído nas métricas**
✅ **Configuração flexível via código ou appsettings.json**

## Uso com Prometheus

As métricas são automaticamente expostas no endpoint `/metrics` do Prometheus e podem ser consultadas usando queries como:

```promql
# Total de requisições por rota
sum(rate(http_requests_total_by_route[5m])) by (route)

# Duração média das requisições
histogram_quantile(0.95, rate(http_request_duration_seconds_by_route_bucket[5m]))

# Taxa de erro por rota
rate(http_requests_errors_total_by_route[5m]) / rate(http_requests_total_by_route[5m])
```
