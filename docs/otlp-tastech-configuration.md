# Configuração OTLP para Tastech Developer

## 🎯 Configuração Específica

Este documento mostra como configurar o Package.Observability para enviar dados via OTLP HTTP Protocol Buffer para o endpoint da Tastech Developer.

## 🔧 Endpoint de Destino

```
https://collector.qas.tastechdeveloper.shop
```

## 📋 Configuração Completa

### **1. Configuração via appsettings.json**

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "ServiceVersion": "1.0.0",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
    "OtlpEndpoint": "https://collector.qas.tastechdeveloper.shop",
    "OtlpProtocol": "HttpProtobuf",
    "RecordExceptions": true,
    "ExcludePaths": ["/metrics", "/health", "/swagger"],
    "EnableRouteMetrics": true,
    "EnableDetailedEndpointMetrics": true,
    "EnableRuntimeInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    "EnableRequestLogging": true,
    "SlowRequestThreshold": 1000,
    "CustomHistogramBuckets": [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10],
    "CustomMetricLabels": {
      "environment": "qas",
      "region": "us-east-1",
      "service-type": "web-api"
    },
    "AdditionalLabels": {
      "team": "observability",
      "version": "1.0.0",
      "cluster": "qas"
    },
    "MetricNames": {
      "HttpRequestsTotal": "http_requests_total_by_route",
      "HttpRequestErrorsTotal": "http_requests_errors_total_by_route",
      "HttpRequestDurationSeconds": "http_request_duration_seconds_by_route"
    }
  }
}
```

### **2. Configuração via Código**

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração OTLP para Tastech Developer
builder.Services.AddObservability(options =>
{
    // Configuração básica
    options.ServiceName = "MinhaAplicacao";
    options.ServiceVersion = "1.0.0";
    
    // Configuração OTLP
    options.OtlpEndpoint = "https://collector.qas.tastechdeveloper.shop";
    options.OtlpProtocol = "HttpProtobuf";
    
    // Configuração de Tracing
    options.EnableTracing = true;
    options.RecordExceptions = true;
    options.ExcludePaths = new List<string> { "/metrics", "/health", "/swagger" };
    options.EnableAspNetCoreInstrumentation = true;
    options.EnableHttpClientInstrumentation = true;
    
    // Configuração de Métricas
    options.EnableMetrics = true;
    options.EnableRouteMetrics = true;
    options.EnableDetailedEndpointMetrics = true;
    options.EnableRuntimeInstrumentation = true;
    options.EnableAspNetCoreInstrumentation = true;
    options.EnableHttpClientInstrumentation = true;
    
    // Buckets personalizados para histograma
    options.CustomHistogramBuckets = new List<double> 
    { 
        0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 
    };
    
    // Labels customizados para métricas
    options.CustomMetricLabels = new Dictionary<string, string>
    {
        { "environment", "qas" },
        { "region", "us-east-1" },
        { "service-type", "web-api" }
    };
    
    // Labels adicionais
    options.AdditionalLabels = new Dictionary<string, string>
    {
        { "team", "observability" },
        { "version", "1.0.0" },
        { "cluster", "qas" }
    };
    
    // Nomes personalizados para métricas
    options.MetricNames = new MetricNamesConfiguration
    {
        HttpRequestsTotal = "http_requests_total_by_route",
        HttpRequestErrorsTotal = "http_requests_errors_total_by_route",
        HttpRequestDurationSeconds = "http_request_duration_seconds_by_route"
    };
    
    // Configuração de Logs
    options.EnableLogging = true;
    options.EnableConsoleLogging = true;
    options.MinimumLogLevel = "Information";
    options.EnableCorrelationId = true;
    options.EnableRequestLogging = true;
    options.SlowRequestThreshold = 1000;
});

var app = builder.Build();

// Adicionar middleware de métricas personalizadas por rota
app.UseCustomRouteMetrics();

app.Run();
```

## 🚀 Configuração por Ambiente

### **Desenvolvimento**

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao-Dev";
    options.ServiceVersion = "1.0.0-dev";
    options.OtlpEndpoint = "https://collector.qas.tastechdeveloper.shop";
    options.OtlpProtocol = "HttpProtobuf";
    
    // Desenvolvimento: logs no console também
    options.EnableConsoleLogging = true;
    options.EnableRequestLogging = true;
    
    // Labels para desenvolvimento
    options.AdditionalLabels = new Dictionary<string, string>
    {
        { "environment", "development" },
        { "team", "observability" }
    };
});
```

### **QA/Staging**

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao-QA";
    options.ServiceVersion = "1.0.0-qa";
    options.OtlpEndpoint = "https://collector.qas.tastechdeveloper.shop";
    options.OtlpProtocol = "HttpProtobuf";
    
    // QA: sem logs no console
    options.EnableConsoleLogging = false;
    
    // Labels para QA
    options.AdditionalLabels = new Dictionary<string, string>
    {
        { "environment", "qa" },
        { "team", "observability" },
        { "cluster", "qas" }
    };
});
```

### **Produção**

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao-Prod";
    options.ServiceVersion = "1.0.0";
    options.OtlpEndpoint = "https://collector.prod.tastechdeveloper.shop"; // Endpoint de produção
    options.OtlpProtocol = "HttpProtobuf";
    
    // Produção: sem logs no console
    options.EnableConsoleLogging = false;
    
    // Labels para produção
    options.AdditionalLabels = new Dictionary<string, string>
    {
        { "environment", "production" },
        { "team", "observability" },
        { "cluster", "prod" },
        { "region", "us-east-1" }
    };
});
```

## 📊 Dados Enviados

### **Traces (Rastreamento)**

- **Requisições HTTP**: Todas as requisições para a aplicação
- **Chamadas HTTP Client**: Requisições outbound
- **Exceções**: Erros e exceções capturadas
- **Duração**: Tempo de execução das operações
- **Correlation ID**: Rastreamento de requisições

### **Métricas**

- **Métricas por Rota**:
  - `http_requests_total_by_route` - Total de requisições por rota
  - `http_requests_errors_total_by_route` - Total de erros por rota
  - `http_request_duration_seconds_by_route` - Duração das requisições por rota

- **Métricas de Runtime .NET**:
  - GC, threads, exceções, etc.

- **Métricas de ASP.NET Core**:
  - Requisições HTTP, duração, status codes

### **Logs**

- **Logs estruturados** com Correlation ID
- **Enrichers**: Informações de processo, thread, ambiente
- **Níveis configuráveis**: Debug, Information, Warning, Error, Fatal

## 🔍 Validação da Configuração

### **Health Check**

```bash
curl http://localhost:5000/health
```

Deve retornar:
```json
{
  "status": "Healthy",
  "checks": {
    "observability": "Healthy",
    "metrics": "Healthy",
    "tracing": "Healthy",
    "logging": "Healthy"
  },
  "configuration": {
    "otlpEndpoint": "https://collector.qas.tastechdeveloper.shop",
    "otlpProtocol": "HttpProtobuf",
    "serviceName": "MinhaAplicacao"
  }
}
```

### **Teste de Conectividade**

```csharp
[Fact]
public async Task OtlpEndpoint_Connectivity_ShouldBeReachable()
{
    using var httpClient = new HttpClient();
    httpClient.Timeout = TimeSpan.FromSeconds(30);
    
    var response = await httpClient.SendAsync(
        new HttpRequestMessage(HttpMethod.Head, "https://collector.qas.tastechdeveloper.shop"));
    
    response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
}
```

## 🛠️ Troubleshooting

### **Problemas Comuns**

1. **Timeout na conectividade**:
   - Verificar se o endpoint está acessível
   - Verificar firewall/proxy
   - Aumentar timeout se necessário

2. **Dados não aparecem no monitor**:
   - Verificar se o protocolo está correto (HttpProtobuf)
   - Verificar se os labels estão configurados
   - Verificar se o ServiceName está correto

3. **Erros de configuração**:
   - Verificar se todas as propriedades estão corretas
   - Verificar se o JSON está válido
   - Verificar se as dependências estão instaladas

### **Logs de Debug**

```csharp
builder.Services.AddObservability(options =>
{
    // ... outras configurações
    
    // Habilitar logs detalhados para debug
    options.EnableConsoleLogging = true;
    options.MinimumLogLevel = "Debug";
});
```

## 📈 Monitoramento

### **Métricas Importantes**

- **Taxa de requisições**: `rate(http_requests_total_by_route[5m])`
- **Taxa de erro**: `rate(http_requests_errors_total_by_route[5m]) / rate(http_requests_total_by_route[5m])`
- **Duração P95**: `histogram_quantile(0.95, rate(http_request_duration_seconds_by_route_bucket[5m]))`

### **Alertas Recomendados**

- Taxa de erro > 5%
- Duração P95 > 1s
- Taxa de requisições < 1 req/min (possível problema)

## 🔐 Segurança

### **Considerações**

- O endpoint usa HTTPS (seguro)
- Não há autenticação configurada (verificar se necessário)
- Dados sensíveis não devem ser incluídos nos labels
- Usar labels apropriados para diferentes ambientes

### **Labels Seguros**

```csharp
options.AdditionalLabels = new Dictionary<string, string>
{
    { "environment", "qa" },        // ✅ OK
    { "team", "observability" },    // ✅ OK
    { "version", "1.0.0" },         // ✅ OK
    // { "api-key", "secret" },     // ❌ NUNCA
    // { "password", "123456" },    // ❌ NUNCA
};
```

Esta configuração garante que todos os dados de telemetria sejam enviados corretamente para o endpoint da Tastech Developer via OTLP HTTP Protocol Buffer.
