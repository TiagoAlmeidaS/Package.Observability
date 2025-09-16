# 📖 Guia de Uso - Package.Observability

Este guia detalha como usar o pacote `Package.Observability` com diferentes configurações e cenários de uso.

## 🎯 Visão Geral

O `Package.Observability` oferece três pilares de observabilidade:
- **Métricas** (Prometheus)
- **Logs** (Serilog + Console/Loki)
- **Tracing** (OpenTelemetry + OTLP)

Cada pilar pode ser configurado independentemente, permitindo flexibilidade total.

## 🚀 Configurações Básicas

### 1. Configuração Mínima (Apenas Console)

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração mínima - apenas console logging
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;      // Desabilita métricas
    options.EnableTracing = false;      // Desabilita tracing
    options.EnableLogging = true;       // Mantém apenas logging
    options.EnableConsoleLogging = true; // Apenas console
    options.LokiUrl = "";              // Remove Loki
    options.OtlpEndpoint = "";         // Remove OTLP
});

var app = builder.Build();
app.Run();
```

### 2. Configuração Completa (Produção)

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuração completa para produção
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.PrometheusPort = 9090;
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki.monitoring.svc.cluster.local:3100";
    options.OtlpEndpoint = "http://jaeger.monitoring.svc.cluster.local:4317";
    options.EnableConsoleLogging = false; // Apenas em desenvolvimento
    options.MinimumLogLevel = "Information";
    options.EnableCorrelationId = true;
    
    // Labels customizados
    options.AdditionalLabels.Add("environment", "production");
    options.AdditionalLabels.Add("version", "1.0.0");
    options.AdditionalLabels.Add("team", "backend");
    
    options.LokiLabels.Add("app", "meu-servico");
    options.LokiLabels.Add("component", "api");
});

var app = builder.Build();
app.Run();
```

## 🔧 Configurações por Cenário

### Cenário 1: Desenvolvimento Local (Sem Infraestrutura Externa)

```json
// appsettings.Development.json
{
  "Observability": {
    "ServiceName": "MeuServico-Dev",
    "EnableMetrics": true,
    "EnableTracing": false,        // Desabilita tracing
    "EnableLogging": true,
    "EnableConsoleLogging": true,  // Apenas console
    "LokiUrl": "",                // Remove Loki
    "OtlpEndpoint": "",           // Remove OTLP
    "MinimumLogLevel": "Debug",
    "AdditionalLabels": {
      "environment": "development"
    }
  }
}
```

### Cenário 2: Apenas Métricas (Sem Logs e Tracing)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = false;
    options.EnableLogging = false;
    options.PrometheusPort = 9090;
    options.AdditionalLabels.Add("service_type", "metrics_only");
});
```

### Cenário 3: Apenas Logs (Sem Métricas e Tracing)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = false;
    options.EnableLogging = true;
    options.EnableConsoleLogging = true;
    options.LokiUrl = "http://loki:3100"; // Ou "" para apenas console
    options.MinimumLogLevel = "Information";
});
```

### Cenário 4: Apenas Tracing (Sem Métricas e Logs)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = true;
    options.EnableLogging = false;
    options.OtlpEndpoint = "http://jaeger:4317";
});
```

## 📋 Configuração via appsettings.json

### Configuração Completa

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "PrometheusPort": 9090,
    
    // Controle de funcionalidades
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    
    // Configurações de métricas
    "EnableRuntimeInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableAspNetCoreInstrumentation": true,
    
    // Configurações de logging
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    
    // Endpoints externos
    "LokiUrl": "http://loki:3100",
    "OtlpEndpoint": "http://jaeger:4317",
    
    // Labels customizados
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

### Configuração Mínima

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": false,
    "EnableTracing": false,
    "EnableLogging": true,
    "EnableConsoleLogging": true
  }
}
```

## 🎛️ Configurações Avançadas

### 1. Configuração por Ambiente

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuração baseada no ambiente
var environment = builder.Environment.EnvironmentName;

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    
    if (environment == "Development")
    {
        // Desenvolvimento: apenas console
        options.EnableMetrics = true;
        options.EnableTracing = false;
        options.EnableLogging = true;
        options.EnableConsoleLogging = true;
        options.LokiUrl = "";
        options.OtlpEndpoint = "";
        options.MinimumLogLevel = "Debug";
    }
    else if (environment == "Staging")
    {
        // Staging: métricas + logs + tracing
        options.EnableMetrics = true;
        options.EnableTracing = true;
        options.EnableLogging = true;
        options.EnableConsoleLogging = false;
        options.LokiUrl = "http://loki-staging:3100";
        options.OtlpEndpoint = "http://jaeger-staging:4317";
        options.MinimumLogLevel = "Information";
    }
    else // Production
    {
        // Produção: configuração completa
        options.EnableMetrics = true;
        options.EnableTracing = true;
        options.EnableLogging = true;
        options.EnableConsoleLogging = false;
        options.LokiUrl = "http://loki.monitoring.svc.cluster.local:3100";
        options.OtlpEndpoint = "http://jaeger.monitoring.svc.cluster.local:4317";
        options.MinimumLogLevel = "Warning";
    }
    
    // Labels baseados no ambiente
    options.AdditionalLabels.Add("environment", environment);
    options.AdditionalLabels.Add("version", "1.0.0");
});

var app = builder.Build();
app.Run();
```

### 2. Configuração com Validação Customizada

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddObservability(builder.Configuration);
}
catch (ObservabilityConfigurationException ex)
{
    // Tratar erros de configuração
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

### 3. Configuração com Health Checks

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(builder.Configuration);

// Health checks são registrados automaticamente
// Mas você pode adicionar mais configurações
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy("Database OK"))
    .AddCheck("external-api", () => HealthCheckResult.Healthy("External API OK"));

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

## 🔍 Uso de Métricas Customizadas

### 1. Criando Métricas

```csharp
using Package.Observability;

public class MeuServico
{
    // Contador de requisições
    private static readonly Counter<int> _requestCounter = 
        ObservabilityMetrics.CreateCounter<int>(
            "MeuServico", 
            "requests_total", 
            "count", 
            "Total de requisições processadas"
        );
    
    // Histograma de duração
    private static readonly Histogram<double> _requestDuration = 
        ObservabilityMetrics.CreateHistogram<double>(
            "MeuServico", 
            "request_duration_seconds", 
            "seconds", 
            "Duração das requisições em segundos"
        );
    
    // Gauge de itens em processamento
    private static readonly ObservableGauge<int> _itemsInProgress = 
        ObservabilityMetrics.CreateObservableGauge<int>(
            "MeuServico", 
            "items_in_progress", 
            "items", 
            "Número de itens sendo processados"
        );
    
    // UpDownCounter para conexões ativas
    private static readonly UpDownCounter<int> _activeConnections = 
        ObservabilityMetrics.CreateUpDownCounter<int>(
            "MeuServico", 
            "active_connections", 
            "connections", 
            "Número de conexões ativas"
        );
}
```

### 2. Usando Métricas em Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private static readonly Counter<int> _produtoRequests = 
        ObservabilityMetrics.CreateCounter<int>("ProdutosAPI", "produto_requests_total", "count", "Total de requisições de produtos");
    
    private static readonly Histogram<double> _produtoDuration = 
        ObservabilityMetrics.CreateHistogram<double>("ProdutosAPI", "produto_duration_seconds", "seconds", "Duração das requisições de produtos");
    
    [HttpGet]
    public async Task<IActionResult> GetProdutos()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var produtos = await ObterProdutos();
            
            _produtoRequests.Add(1, new KeyValuePair<string, object?>("status", "success"));
            
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _produtoRequests.Add(1, new KeyValuePair<string, object?>("status", "error"));
            throw;
        }
        finally
        {
            _produtoDuration.Record(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
```

## 📊 Uso de Tracing

### 1. Criando Activities

```csharp
using Package.Observability;

public class MeuServico
{
    public async Task ProcessarItem(string itemId)
    {
        // Criar activity para tracing
        using var activity = ActivitySourceFactory.StartActivity(
            "MeuServico", 
            "ProcessarItem",
            ActivityKind.Internal
        );
        
        // Adicionar tags
        activity?.SetTag("item.id", itemId);
        activity?.SetTag("item.type", "documento");
        
        try
        {
            // Seu código aqui
            await ProcessarDocumento(itemId);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }
}
```

### 2. Tracing em HTTP Clients

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GetDataAsync(string endpoint)
    {
        using var activity = ActivitySourceFactory.StartActivity(
            "ApiClient", 
            "GetData",
            ActivityKind.Client
        );
        
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", endpoint);
        
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("http.status_text", response.StatusCode.ToString());
            
            if (response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {response.StatusCode}");
                throw new HttpRequestException($"HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }
}
```

## 📝 Uso de Logs Estruturados

### 1. Logging Básico

```csharp
public class MeuServico
{
    private readonly ILogger<MeuServico> _logger;
    
    public MeuServico(ILogger<MeuServico> logger)
    {
        _logger = logger;
    }
    
    public async Task ProcessarItem(string itemId, string tipo)
    {
        _logger.LogInformation("Iniciando processamento do item {ItemId} do tipo {Tipo}", itemId, tipo);
        
        try
        {
            // Seu código aqui
            await ProcessarDocumento(itemId);
            
            _logger.LogInformation("Item {ItemId} processado com sucesso", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar item {ItemId} do tipo {Tipo}", itemId, tipo);
            throw;
        }
    }
}
```

### 2. Logging com Correlation ID

```csharp
[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly ILogger<PedidosController> _logger;
    
    public PedidosController(ILogger<PedidosController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoRequest request)
    {
        // Correlation ID é adicionado automaticamente se EnableCorrelationId = true
        _logger.LogInformation("Criando pedido para cliente {ClienteId} com {QuantidadeItens} itens", 
            request.ClienteId, request.Itens.Count);
        
        try
        {
            var pedido = await ProcessarPedido(request);
            
            _logger.LogInformation("Pedido {PedidoId} criado com sucesso para cliente {ClienteId}", 
                pedido.Id, request.ClienteId);
            
            return CreatedAtAction(nameof(ObterPedido), new { id = pedido.Id }, pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido para cliente {ClienteId}", request.ClienteId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
```

## 🐳 Configurações Docker

### 1. Dockerfile para Aplicação

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9090  # Porta do Prometheus

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MeuServico.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeuServico.dll"]
```

### 2. Docker Compose para Desenvolvimento

```yaml
version: '3.8'

services:
  meu-servico:
    build: .
    ports:
      - "5000:80"
      - "9090:9090"  # Prometheus metrics
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

### 3. Docker Compose para Produção

```yaml
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

## 🔧 Configurações Específicas por Ambiente

### 1. appsettings.Development.json

```json
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

### 2. appsettings.Staging.json

```json
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

### 3. appsettings.Production.json

```json
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
      "component": "api",
      "tier": "backend"
    }
  }
}
```

## 🚨 Tratamento de Erros

### 1. Validação de Configuração

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddObservability(builder.Configuration);
}
catch (ObservabilityConfigurationException ex)
{
    // Log do erro
    Console.WriteLine($"Erro de configuração de observabilidade: {ex.Message}");
    
    // Configuração de fallback
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

### 2. Health Checks com Fallback

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(builder.Configuration);

// Health checks customizados
builder.Services.AddHealthChecks()
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

app.MapHealthChecks("/health");

app.Run();
```

## 📊 Monitoramento e Alertas

### 1. Métricas Importantes para Monitorar

- `http_requests_total` - Total de requisições HTTP
- `http_request_duration_seconds` - Duração das requisições
- `dotnet_gc_collections_total` - Coleta de lixo
- `dotnet_threadpool_threads_total` - Threads do pool
- `dotnet_exceptions_total` - Total de exceções

### 2. Logs Importantes para Alertas

- `ERROR` - Erros críticos
- `WARN` - Avisos importantes
- `FATAL` - Erros fatais

### 3. Traces Importantes

- Duração de operações críticas
- Erros em cadeias de chamadas
- Performance de queries de banco

## 🎯 Melhores Práticas

### 1. Configuração

- Use configuração baseada em ambiente
- Sempre tenha um fallback para configuração mínima
- Valide configurações em startup
- Use labels consistentes entre ambientes

### 2. Métricas

- Crie métricas com nomes descritivos
- Use labels para dimensões importantes
- Evite alta cardinalidade em labels
- Documente suas métricas customizadas

### 3. Logs

- Use logs estruturados com parâmetros
- Inclua contexto relevante (IDs, timestamps)
- Use níveis de log apropriados
- Evite logs excessivos em produção

### 4. Tracing

- Crie activities para operações importantes
- Use tags para contexto adicional
- Mantenha traces com duração razoável
- Use correlation IDs para rastreamento

## 🔍 Troubleshooting

### 1. Problemas Comuns

**Métricas não aparecem no Prometheus:**
- Verifique se `EnableMetrics = true`
- Confirme se a porta está correta
- Verifique se o Prometheus está configurado para fazer scrape

**Logs não aparecem no Loki:**
- Verifique se `LokiUrl` está correto
- Confirme se o Loki está rodando
- Verifique se `EnableLogging = true`

**Traces não aparecem no Jaeger:**
- Verifique se `OtlpEndpoint` está correto
- Confirme se o Jaeger está rodando
- Verifique se `EnableTracing = true`

### 2. Debug de Configuração

```csharp
// Adicione este código para debug
var configuration = builder.Configuration.GetSection("Observability");
var options = configuration.Get<ObservabilityOptions>();

Console.WriteLine($"ServiceName: {options.ServiceName}");
Console.WriteLine($"EnableMetrics: {options.EnableMetrics}");
Console.WriteLine($"EnableTracing: {options.EnableTracing}");
Console.WriteLine($"EnableLogging: {options.EnableLogging}");
Console.WriteLine($"LokiUrl: {options.LokiUrl}");
Console.WriteLine($"OtlpEndpoint: {options.OtlpEndpoint}");
```

## 📚 Exemplos Completos

### 1. ASP.NET Core Web API Completa

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

Este guia cobre todos os aspectos de uso do `Package.Observability`. Para mais informações, consulte a documentação da API ou abra uma issue no repositório.
