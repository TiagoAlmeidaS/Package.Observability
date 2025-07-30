# Package.Observability

[![NuGet](https://img.shields.io/nuget/v/Package.Observability.svg)](https://www.nuget.org/packages/Package.Observability)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Um pacote completo de observabilidade para aplicações .NET 8, fornecendo métricas (Prometheus), logs estruturados (Serilog + Loki) e rastreamento distribuído (OpenTelemetry) de forma integrada e configurável.

## 🚀 Características

- **Métricas**: Coleta automática de métricas de runtime, ASP.NET Core e HTTP Client com exportação para Prometheus
- **Logs Estruturados**: Configuração automática do Serilog com suporte a Console e Grafana Loki
- **Rastreamento Distribuído**: Instrumentação OpenTelemetry com exportação OTLP
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

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `ServiceName` | `string` | `"DefaultService"` | Nome do serviço para identificação |
| `PrometheusPort` | `int` | `9090` | Porta do endpoint de métricas Prometheus |
| `EnableMetrics` | `bool` | `true` | Habilita coleta de métricas |
| `EnableTracing` | `bool` | `true` | Habilita rastreamento distribuído |
| `EnableLogging` | `bool` | `true` | Habilita logs estruturados |
| `LokiUrl` | `string` | `"http://localhost:3100"` | URL do Grafana Loki |
| `OtlpEndpoint` | `string` | `"http://localhost:4317"` | Endpoint OTLP para traces |
| `EnableConsoleLogging` | `bool` | `true` | Habilita logs no console |
| `MinimumLogLevel` | `string` | `"Information"` | Nível mínimo de log |
| `EnableCorrelationId` | `bool` | `true` | Habilita Correlation ID automático |
| `AdditionalLabels` | `Dictionary<string, string>` | `{}` | Labels adicionais para métricas |
| `LokiLabels` | `Dictionary<string, string>` | `{}` | Labels customizados para Loki |
| `EnableRuntimeInstrumentation` | `bool` | `true` | Habilita métricas de runtime .NET |
| `EnableHttpClientInstrumentation` | `bool` | `true` | Habilita instrumentação HTTP Client |
| `EnableAspNetCoreInstrumentation` | `bool` | `true` | Habilita instrumentação ASP.NET Core |

## 🎯 Uso Avançado

### Configuração por Código

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

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "4317:4317"
    environment:
      - COLLECTOR_OTLP_ENABLED=true

volumes:
  grafana-storage:
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
- **Métricas customizadas**: Definidas pela aplicação

### Logs Estruturados

- **Console**: Formatação legível para desenvolvimento
- **Loki**: Agregação centralizada para produção
- **Correlation ID**: Rastreamento de requisições
- **Enrichers**: Informações de processo, thread, ambiente

### Traces Distribuídos

- **OpenTelemetry**: Padrão da indústria
- **OTLP Export**: Compatível com Jaeger, Zipkin, etc.
- **Instrumentação automática**: ASP.NET Core, HTTP Client
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
