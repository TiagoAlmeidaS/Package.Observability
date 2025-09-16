# Exemplos

Exemplos práticos de uso do Package.Observability.

## 🚀 Exemplos Básicos

### 1. Configuração Mínima

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração mínima
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

### 2. Configuração por Código

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
});

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

## 🎯 Exemplos por Tipo de Aplicação

### ASP.NET Core Web API

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();

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
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicação iniciada");

await host.RunAsync();
```

## 📊 Exemplos de Métricas

### 1. Contador de Requisições

```csharp
using Package.Observability;

public class MeuController : ControllerBase
{
    private static readonly Counter<int> _requestCounter = 
        ObservabilityMetrics.CreateCounter<int>("MinhaAplicacao", "requests_total", "count", "Total de requisições");

    [HttpGet]
    public IActionResult Get()
    {
        _requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/api/meu-endpoint"));
        return Ok();
    }
}
```

### 2. Histograma de Duração

```csharp
using Package.Observability;
using System.Diagnostics;

public class MeuController : ControllerBase
{
    private static readonly Histogram<double> _requestDuration = 
        ObservabilityMetrics.CreateHistogram<double>("MinhaAplicacao", "request_duration", "ms", "Duração das requisições");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Seu código aqui
            await ProcessarRequisicao();
            
            return Ok();
        }
        finally
        {
            _requestDuration.Record(stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### 3. Gauge de Conexões Ativas

```csharp
using Package.Observability;

public class MeuServico
{
    private static readonly ObservableGauge<int> _activeConnections = 
        ObservabilityMetrics.CreateObservableGauge<int>("MinhaAplicacao", "active_connections", "connections", "Conexões ativas");

    private static int _connectionCount = 0;

    public void AdicionarConexao()
    {
        Interlocked.Increment(ref _connectionCount);
    }

    public void RemoverConexao()
    {
        Interlocked.Decrement(ref _connectionCount);
    }

    private static int GetActiveConnections() => _connectionCount;
}
```

### 4. Contador Up-Down para Fila

```csharp
using Package.Observability;

public class ProcessadorFila
{
    private static readonly UpDownCounter<int> _queueSize = 
        ObservabilityMetrics.CreateUpDownCounter<int>("MinhaAplicacao", "queue_size", "items", "Tamanho da fila");

    public void AdicionarItem()
    {
        _queueSize.Add(1);
    }

    public void ProcessarItem()
    {
        _queueSize.Add(-1);
    }
}
```

## 🔍 Exemplos de Traces

### 1. Trace Básico

```csharp
using Package.Observability;

public class MeuServico
{
    public async Task ProcessarDados()
    {
        using var activity = ActivitySourceFactory.StartActivity("MinhaAplicacao", "ProcessarDados");
        
        activity?.SetTag("operation.type", "data_processing");
        activity?.SetTag("data.size", "large");
        
        try
        {
            // Seu código aqui
            await ProcessarDadosInternos();
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

### 2. Trace com Span Filho

```csharp
using Package.Observability;

public class MeuServico
{
    public async Task ProcessarRequisicao()
    {
        using var parentActivity = ActivitySourceFactory.StartActivity("MinhaAplicacao", "ProcessarRequisicao");
        
        // Span filho 1
        using var validationActivity = ActivitySourceFactory.StartActivity("MinhaAplicacao", "ValidarDados");
        validationActivity?.SetTag("validation.type", "input");
        await ValidarDados();
        validationActivity?.SetStatus(ActivityStatusCode.Ok);
        
        // Span filho 2
        using var processingActivity = ActivitySourceFactory.StartActivity("MinhaAplicacao", "ProcessarDados");
        processingActivity?.SetTag("processing.type", "business_logic");
        await ProcessarDados();
        processingActivity?.SetStatus(ActivityStatusCode.Ok);
        
        parentActivity?.SetStatus(ActivityStatusCode.Ok);
    }
}
```

### 3. Trace com Contexto de Requisição

```csharp
using Package.Observability;

public class MeuController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        using var activity = ActivitySourceFactory.StartActivity("MinhaAplicacao", "GetEndpoint");
        
        // Adicionar informações da requisição
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", Request.Path);
        activity?.SetTag("user.id", GetUserId());
        
        try
        {
            var resultado = await ProcessarRequisicao();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return StatusCode(500, "Erro interno");
        }
    }
}
```

## 📝 Exemplos de Logs

### 1. Logs Estruturados Básicos

```csharp
using Microsoft.Extensions.Logging;

public class MeuServico
{
    private readonly ILogger<MeuServico> _logger;

    public MeuServico(ILogger<MeuServico> logger)
    {
        _logger = logger;
    }

    public async Task ProcessarDados()
    {
        _logger.LogInformation("Iniciando processamento de dados");
        
        try
        {
            await ProcessarDadosInternos();
            _logger.LogInformation("Processamento concluído com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar dados");
            throw;
        }
    }
}
```

### 2. Logs com Propriedades Estruturadas

```csharp
using Microsoft.Extensions.Logging;

public class MeuController : ControllerBase
{
    private readonly ILogger<MeuController> _logger;

    public MeuController(ILogger<MeuController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogInformation("Processando requisição GET para ID {Id}", id);
        
        try
        {
            var resultado = await ObterDados(id);
            
            _logger.LogInformation("Requisição processada com sucesso. ID: {Id}, Resultado: {Resultado}", 
                id, resultado);
            
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição para ID {Id}", id);
            return StatusCode(500, "Erro interno");
        }
    }
}
```

### 3. Logs com Correlation ID

```csharp
using Microsoft.Extensions.Logging;

public class MeuServico
{
    private readonly ILogger<MeuServico> _logger;

    public MeuServico(ILogger<MeuServico> logger)
    {
        _logger = logger;
    }

    public async Task ProcessarRequisicao(string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = "ProcessarRequisicao"
        });

        _logger.LogInformation("Iniciando processamento");
        
        try
        {
            await ProcessarDados();
            _logger.LogInformation("Processamento concluído");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento");
            throw;
        }
    }
}
```

## 🔧 Exemplos de Configuração

### 1. Configuração por Ambiente

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

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
else
{
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MinhaAplicacao";
        options.EnableConsoleLogging = false;
        options.MinimumLogLevel = "Information";
        options.LokiUrl = "http://loki.monitoring.svc.cluster.local:3100";
        options.OtlpEndpoint = "http://jaeger.monitoring.svc.cluster.local:4317";
        options.AdditionalLabels.Add("environment", "production");
    });
}

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

### 2. Configuração com Validação

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(builder.Configuration);

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

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

### 3. Configuração com Múltiplas Fontes

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração base
builder.Services.AddObservability(builder.Configuration);

// Configuração adicional
builder.Services.Configure<ObservabilityOptions>(options =>
{
    options.AdditionalLabels.Add("deployment", "v1.0.0");
    options.LokiLabels.Add("deployment", "v1.0.0");
});

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.Run();
```

## 🐳 Exemplos de Docker

### 1. Dockerfile Básico

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MinhaAplicacao.csproj", "."]
RUN dotnet restore "MinhaAplicacao.csproj"
COPY . .
RUN dotnet build "MinhaAplicacao.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinhaAplicacao.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinhaAplicacao.dll"]
```

### 2. Docker Compose com Observabilidade

```yaml
version: '3.8'

services:
  minha-aplicacao:
    build: .
    ports:
      - "80:80"
      - "9090:9090"
    environment:
      - Observability__ServiceName=MinhaAplicacao
      - Observability__LokiUrl=http://loki:3100
      - Observability__OtlpEndpoint=http://jaeger:4317
    depends_on:
      - loki
      - jaeger

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9091:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin

  loki:
    image: grafana/loki:latest
    ports:
      - "3100:3100"

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "4317:4317"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
```

## 🧪 Exemplos de Testes

### 1. Teste de Métricas

```csharp
using Package.Observability;
using Xunit;

public class MetricTests
{
    [Fact]
    public void Should_Create_Counter()
    {
        var counter = ObservabilityMetrics.CreateCounter<int>("TestService", "test_counter");
        Assert.NotNull(counter);
    }

    [Fact]
    public void Should_Create_Histogram()
    {
        var histogram = ObservabilityMetrics.CreateHistogram<double>("TestService", "test_histogram");
        Assert.NotNull(histogram);
    }
}
```

### 2. Teste de Traces

```csharp
using Package.Observability;
using Xunit;

public class TraceTests
{
    [Fact]
    public void Should_Create_Activity()
    {
        using var activity = ActivitySourceFactory.StartActivity("TestService", "TestActivity");
        Assert.NotNull(activity);
        Assert.Equal("TestActivity", activity.DisplayName);
    }
}
```

## 📚 Recursos Adicionais

- [Guia de Início Rápido](getting-started.md)
- [Configuração](configuration.md)
- [API Reference](api-reference.md)
- [Troubleshooting](troubleshooting.md)
