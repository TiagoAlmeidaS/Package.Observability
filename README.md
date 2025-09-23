# Package.Observability

[![NuGet](https://img.shields.io/nuget/v/Package.Observability.svg)](https://www.nuget.org/packages/Package.Observability)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Um pacote completo de observabilidade para aplicações .NET 8, fornecendo métricas (Prometheus), logs estruturados (Serilog + Loki) e rastreamento distribuído (OpenTelemetry) de forma integrada e configurável.

## 🚀 Características

- **Métricas**: Coleta automática de métricas de runtime, ASP.NET Core e HTTP Client com exportação para Prometheus
- **Logs Estruturados**: Configuração automática do Serilog com suporte a Console e Grafana Loki
- **Rastreamento Distribuído**: Instrumentação OpenTelemetry com exportação OTLP para Tempo via Collector
- **Configuração Flexível**: Configuração via `appsettings.json` ou código
- **Correlation ID**: Suporte automático para correlação de logs e traces
- **Fácil Integração**: Uma linha de código para configurar toda a observabilidade

## 📦 Instalação

```bash
dotnet add package Package.Observability
```

## 🔧 Configuração Rápida

### 1. Configure o `appsettings.json`

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
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
      "version": "1.0.0"
    },
    "LokiLabels": {
      "app": "meu-servico",
      "team": "backend"
    }
  }
}
```

### 2. Configure no `Program.cs`

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Adiciona observabilidade completa
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

app.Run();
```

## 📋 Opções de Configuração

### Configurações Básicas

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `ServiceName` | `string` | `"DefaultService"` | Nome do serviço para identificação |
| `ServiceVersion` | `string` | `"1.0.0"` | Versão do serviço para OpenTelemetry |
| `PrometheusPort` | `int` | `9090` | Porta do endpoint de métricas Prometheus |
| `EnableMetrics` | `bool` | `true` | Habilita coleta de métricas |
| `EnableTracing` | `bool` | `true` | Habilita rastreamento distribuído |
| `EnableLogging` | `bool` | `true` | Habilita logs estruturados |
| `LokiUrl` | `string` | `"http://localhost:3100"` | URL do Grafana Loki |
| `OtlpEndpoint` | `string` | `"http://localhost:4317"` | Endpoint OTLP para traces |
| `OtlpProtocol` | `string` | `"Grpc"` | Protocolo OTLP (Grpc ou HttpProtobuf) |
| `CollectorEndpoint` | `string` | `"http://localhost:4317"` | Endpoint do OpenTelemetry Collector |
| `TempoEndpoint` | `string` | `"http://localhost:3200"` | Endpoint do Tempo para traces |
| `EnableConsoleLogging` | `bool` | `true` | Habilita logs no console |
| `MinimumLogLevel` | `string` | `"Information"` | Nível mínimo de log |
| `EnableCorrelationId` | `bool` | `true` | Habilita Correlation ID automático |
| `AdditionalLabels` | `Dictionary<string, string>` | `{}` | Labels adicionais para métricas |
| `LokiLabels` | `Dictionary<string, string>` | `{}` | Labels customizados para Loki |
| `EnableRuntimeInstrumentation` | `bool` | `true` | Habilita métricas de runtime .NET |
| `EnableHttpClientInstrumentation` | `bool` | `true` | Habilita instrumentação HTTP Client |
| `EnableAspNetCoreInstrumentation` | `bool` | `true` | Habilita instrumentação ASP.NET Core |

### Configurações Avançadas de Tracing

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `RecordExceptions` | `bool` | `true` | Habilita gravação de exceções no tracing |
| `ExcludePaths` | `List<string>` | `["/metrics", "/health"]` | Caminhos excluídos do tracing |

### Configurações de Métricas Personalizadas

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `EnableRouteMetrics` | `bool` | `true` | Habilita métricas personalizadas por rota |
| `EnableDetailedEndpointMetrics` | `bool` | `true` | Habilita informações detalhadas de endpoint |
| `CustomHistogramBuckets` | `List<double>` | `[]` | Buckets personalizados para histograma |
| `CustomMetricLabels` | `Dictionary<string, string>` | `{}` | Labels customizados para métricas |
| `MetricNames` | `MetricNamesConfiguration` | `new()` | Configuração de nomes de métricas |

## 📚 Documentação Detalhada

Para configurações avançadas e exemplos específicos, consulte:

- **[Documentação Completa](docs/README.md)** - Índice da documentação
- **[Quick Start](docs/quick-start.md)** - Comece em 30 segundos
- **[Guia de Uso Completo](docs/usage-guide.md)** - Documentação detalhada de uso
- **[Exemplos de Configuração](docs/configuration-examples.md)** - Configurações para diferentes cenários
- **[Análise de Arquitetura OTLP](docs/otlp-architecture-analysis.md)** - OTLP vs. configurações diretas
- **[Estratégias de Configuração](docs/configuration-strategies.md)** - Diferentes abordagens de configuração
- **[Diagramas de Arquitetura](docs/architecture-diagrams.md)** - Visualização das arquiteturas
- **[FAQ](docs/faq.md)** - Perguntas frequentes
- **[Exemplo Sem Loki](examples/without-loki-example.md)** - Como usar sem Loki

## 🔧 Configurações Flexíveis

O pacote oferece máxima flexibilidade - você pode usar apenas os componentes que precisar:

### Apenas Logs (Sem Loki)
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;      // Sem métricas
    options.EnableTracing = false;      // Sem tracing
    options.EnableLogging = true;       // Apenas logs
    options.EnableConsoleLogging = true; // Apenas console
    options.LokiUrl = "";              // Remove Loki
});
```

### Apenas Métricas (Sem Logs e Tracing)
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;       // Apenas métricas
    options.EnableTracing = false;      // Sem tracing
    options.EnableLogging = false;      // Sem logs
    options.PrometheusPort = 9090;     // Porta do Prometheus
});
```

### Apenas Tracing (Sem Métricas e Logs)
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;      // Sem métricas
    options.EnableTracing = true;       // Apenas tracing
    options.EnableLogging = false;      // Sem logs
    options.CollectorEndpoint = "http://otel-collector:4317";
});
```

### Configuração Completa (Produção)
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.CollectorEndpoint = "http://otel-collector:4317";
});
```

## 🎯 Uso Avançado

### Configuração com Exportação OTLP (Recomendado para Produção)

Para configuração completa com exportação OTLP via gRPC, similar ao exemplo fornecido:

```csharp
builder.Services.AddObservability(options =>
{
    // Configuração básica
    options.ServiceName = "testeLoki3";
    options.ServiceVersion = "1.0.0";
    
    // Configuração de Tracing com OTLP
    options.EnableTracing = true;
    options.OtlpEndpoint = "http://localhost:4317";
    options.OtlpProtocol = "Grpc"; // ou "HttpProtobuf"
    options.RecordExceptions = true;
    options.ExcludePaths = new List<string> { "/metrics", "/health" };
    
    // Configuração de Métricas
    options.EnableMetrics = true;
    options.EnableRouteMetrics = true; // Métricas personalizadas por rota
    options.EnableDetailedEndpointMetrics = true;
    
    // Buckets personalizados para histograma (padrão: 5ms a ~163s)
    options.CustomHistogramBuckets = new List<double> 
    { 
        0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 
    };
    
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
    
    // Configuração de Logs
    options.EnableLogging = true;
    options.LokiUrl = "http://localhost:3100";
    options.EnableConsoleLogging = true;
});

var app = builder.Build();

// Adicionar middleware de métricas personalizadas por rota
app.UseCustomRouteMetrics();
```

### Configuração via appsettings.json (OTLP)

```json
{
  "Observability": {
    "ServiceName": "testeLoki3",
    "ServiceVersion": "1.0.0",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
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
    },
    "LokiUrl": "http://localhost:3100",
    "EnableConsoleLogging": true
  }
}
```

### Configuração por Código (Básica)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuBot";
    options.PrometheusPort = 9110;
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.CollectorEndpoint = "http://otel-collector:4317";
    options.AdditionalLabels.Add("bot-type", "fishing");
    options.LokiLabels.Add("component", "worker");
});
```

### Criando Métricas Customizadas

```csharp
using Package.Observability;

public class MeuServico
{
    private static readonly Counter<int> _requestCounter = 
        ObservabilityMetrics.CreateCounter<int>("MeuServico", "requests_total", "count", "Total de requisições");
    
    private static readonly Histogram<double> _requestDuration = 
        ObservabilityMetrics.CreateHistogram<double>("MeuServico", "request_duration", "ms", "Duração das requisições");

    public async Task ProcessarRequisicao()
    {
        using var activity = ActivitySourceFactory.StartActivity("MeuServico", "ProcessarRequisicao");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Seu código aqui
            await Task.Delay(100);
            
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
        }
        catch (Exception ex)
        {
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _requestDuration.Record(stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Usando Logs Estruturados

```csharp
using Microsoft.Extensions.Logging;

public class MeuController : ControllerBase
{
    private readonly ILogger<MeuController> _logger;

    public MeuController(ILogger<MeuController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Processando requisição GET {Endpoint}", "/api/dados");
        
        try
        {
            var dados = await ObterDados();
            
            _logger.LogInformation("Requisição processada com sucesso. {Count} itens retornados", dados.Count);
            
            return Ok(dados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição GET {Endpoint}", "/api/dados");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
```

### Métricas Personalizadas por Rota

O pacote inclui um middleware especializado para métricas personalizadas por rota, similar ao exemplo fornecido:

```csharp
// Program.cs
builder.Services.AddObservability(options =>
{
    options.ServiceName = "testeLoki3";
    options.ServiceVersion = "1.0.0";
    options.EnableRouteMetrics = true;
    options.EnableDetailedEndpointMetrics = true;
    
    // Buckets personalizados (padrão: 5ms a ~163s)
    options.CustomHistogramBuckets = new List<double> 
    { 
        0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 
    };
    
    // Labels customizados
    options.CustomMetricLabels = new Dictionary<string, string>
    {
        { "environment", "production" },
        { "region", "us-east-1" }
    };
});

var app = builder.Build();

// Adicionar middleware de métricas personalizadas
app.UseCustomRouteMetrics();
```

#### Métricas Geradas Automaticamente

O middleware gera as seguintes métricas com labels específicos:

1. **`http_requests_total_by_route`** - Contador de requisições HTTP
   - Labels: `method`, `endpoint`, `route`
   - Exemplo: `http_requests_total_by_route{method="GET",endpoint="WeatherForecastController.Get",route="/weatherforecast"}`

2. **`http_requests_errors_total_by_route`** - Contador de erros HTTP
   - Labels: `method`, `endpoint`, `route`
   - Exemplo: `http_requests_errors_total_by_route{method="GET",endpoint="WeatherForecastController.Get",route="/weatherforecast"}`

3. **`http_request_duration_seconds_by_route`** - Histograma de duração
   - Labels: `method`, `endpoint`, `route`
   - Buckets: Configuráveis (padrão: buckets exponenciais)

#### Consultas Prometheus

```promql
# Total de requisições por rota
sum(rate(http_requests_total_by_route[5m])) by (route)

# Duração média das requisições (95º percentil)
histogram_quantile(0.95, rate(http_request_duration_seconds_by_route_bucket[5m]))

# Taxa de erro por rota
rate(http_requests_errors_total_by_route[5m]) / rate(http_requests_total_by_route[5m])

# Requisições por método e rota
sum(rate(http_requests_total_by_route[5m])) by (method, route)
```

## 🐳 Docker Compose para Desenvolvimento

Crie um `docker-compose.yml` para executar a stack de observabilidade localmente:

```yaml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9091:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana

  loki:
    image: grafana/loki:latest
    ports:
      - "3100:3100"
    command: -config.file=/etc/loki/local-config.yaml

  tempo:
    image: grafana/tempo:latest
    ports:
      - "3200:3200"    # Tempo HTTP
      - "4317:4317"    # OTLP gRPC receiver
      - "4318:4318"    # OTLP HTTP receiver
    volumes:
      - ./observability/tempo.yml:/etc/tempo.yml
      - tempo-data:/tmp/tempo
    command: -config.file=/etc/tempo.yml

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    ports:
      - "8888:8888"    # Prometheus metrics
      - "8889:8889"    # Prometheus exporter
      - "13133:13133"  # Health check
      - "1777:1777"    # pprof
      - "55679:55679"  # zpages
    volumes:
      - ./observability/otel-collector.yml:/etc/otel-collector.yml
    command: ["--config=/etc/otel-collector.yml"]
    depends_on:
      - tempo
      - loki
      - prometheus

volumes:
  grafana-storage:
  tempo-data:
```

### Configuração do Prometheus (`prometheus.yml`)

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'meu-servico'
    static_configs:
      - targets: ['host.docker.internal:9090']
```

## 📊 Endpoints Disponíveis

Após configurar o pacote, os seguintes endpoints estarão disponíveis:

- **Métricas Prometheus**: `http://localhost:{PrometheusPort}/metrics`
- **Health Check** (se configurado): `http://localhost:5000/health`

## 🔍 Monitoramento

### Métricas Coletadas Automaticamente

- **Runtime .NET**: GC, threads, exceções, etc.
- **ASP.NET Core**: Requisições HTTP, duração, status codes
- **HTTP Client**: Requisições outbound, duração, status codes
- **Métricas por Rota** (quando `UseCustomRouteMetrics()` é usado):
  - `http_requests_total_by_route` - Total de requisições por rota
  - `http_requests_errors_total_by_route` - Total de erros por rota
  - `http_request_duration_seconds_by_route` - Duração das requisições por rota
- **Métricas customizadas**: Definidas pela aplicação

### Logs Estruturados

- **Console**: Formatação legível para desenvolvimento
- **Loki**: Agregação centralizada para produção
- **Correlation ID**: Rastreamento de requisições
- **Enrichers**: Informações de processo, thread, ambiente

### Traces Distribuídos

- **OpenTelemetry**: Padrão da indústria
- **Tempo**: Armazenamento e consulta de traces
- **OTLP Export**: Via OpenTelemetry Collector (gRPC ou HTTP)
- **Instrumentação automática**: ASP.NET Core, HTTP Client
- **Configurações avançadas**:
  - Gravação de exceções (`RecordExceptions`)
  - Filtros de path (`ExcludePaths`)
  - Protocolo OTLP configurável (`OtlpProtocol`)
- **Traces customizados**: Via ActivitySource

## 🚀 Exemplos de Projetos

### ASP.NET Core Web API

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

app.UseRouting();

// Adicionar middleware de métricas personalizadas por rota
app.UseCustomRouteMetrics();

app.MapControllers();

app.Run();
```

### Worker Service

```csharp
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MeuWorker>();
builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();
host.Run();
```

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();

// Seu código aqui
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicação iniciada");

await host.RunAsync();
```

## 🛠️ Desenvolvimento

### Pré-requisitos

- .NET 8 SDK
- Docker (para stack de observabilidade)

### Build Local

```bash
dotnet build
dotnet pack --configuration Release
```

### Testes

```bash
dotnet test
```

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE).

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor, abra uma issue ou pull request.

## 📞 Suporte

Para dúvidas ou problemas, abra uma [issue](https://github.com/your-org/Package.Observability/issues) no GitHub.
