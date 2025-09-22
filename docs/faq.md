# ❓ FAQ - Package.Observability

Perguntas frequentes sobre o uso do `Package.Observability`.

## 🚀 Configuração Básica

### P: Como instalar o pacote?

**R:** Use o comando do .NET CLI:
```bash
dotnet add package Package.Observability
```

### P: Qual é a configuração mínima?

**R:** A configuração mínima é:
```csharp
builder.Services.AddObservability(builder.Configuration);
```

Com `appsettings.json`:
```json
{
  "Observability": {
    "ServiceName": "MeuServico"
  }
}
```

### P: Posso usar apenas logs sem Loki?

**R:** Sim! Configure `LokiUrl = ""` e `EnableConsoleLogging = true`:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = false;
    options.EnableLogging = true;
    options.EnableConsoleLogging = true;
    options.LokiUrl = ""; // Remove Loki
});
```

### P: Posso desabilitar métricas?

**R:** Sim! Configure `EnableMetrics = false`:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false; // Sem métricas
    options.EnableTracing = true;
    options.EnableLogging = true;
});
```

### P: Posso usar apenas tracing?

**R:** Sim! Configure `EnableMetrics = false` e `EnableLogging = false`:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = true;
    options.EnableLogging = false;
    options.CollectorEndpoint = "http://otel-collector:4317";
});
```

## 🔧 Configuração Avançada

### P: Como configurar para desenvolvimento?

**R:** Use esta configuração:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico-Dev";
    options.EnableMetrics = true;           // Métricas locais
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = true;           // Logs no console
    options.EnableConsoleLogging = true;    // Apenas console
    options.LokiUrl = "";                  // Sem Loki
    options.OtlpEndpoint = "";             // Sem OTLP
    options.MinimumLogLevel = "Debug";     // Logs detalhados
});
```

### P: Como configurar para produção?

**R:** Use esta configuração:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.EnableConsoleLogging = false;   // Sem console em produção
    options.LokiUrl = "http://loki:3100";
    options.CollectorEndpoint = "http://otel-collector:4317";
    options.MinimumLogLevel = "Information";
});
```

### P: Como configurar por ambiente?

**R:** Use diferentes arquivos `appsettings`:

**appsettings.Development.json**:
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
    "MinimumLogLevel": "Debug"
  }
}
```

**appsettings.Production.json**:
```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "EnableConsoleLogging": false,
    "LokiUrl": "http://loki:3100",
    "CollectorEndpoint": "http://otel-collector:4317",
    "MinimumLogLevel": "Information"
  }
}
```

### P: Como usar configuração por código?

**R:** Use a sobrecarga com `Action<ObservabilityOptions>`:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.PrometheusPort = 9090;
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.CollectorEndpoint = "http://otel-collector:4317";
    options.AdditionalLabels.Add("environment", "production");
    options.LokiLabels.Add("app", "meu-servico");
});
```

## 📊 Métricas

### P: Como criar métricas customizadas?

**R:** Use a classe `ObservabilityMetrics`:
```csharp
// Contador
private static readonly Counter<int> _requestCounter = 
    ObservabilityMetrics.CreateCounter<int>("MeuServico", "requests_total", "count", "Total de requisições");

// Histograma
private static readonly Histogram<double> _requestDuration = 
    ObservabilityMetrics.CreateHistogram<double>("MeuServico", "request_duration_seconds", "seconds", "Duração das requisições");

// Gauge
private static readonly ObservableGauge<int> _itemsInProgress = 
    ObservabilityMetrics.CreateObservableGauge<int>("MeuServico", "items_in_progress", "items", "Itens em processamento");

// UpDownCounter
private static readonly UpDownCounter<int> _activeConnections = 
    ObservabilityMetrics.CreateUpDownCounter<int>("MeuServico", "active_connections", "connections", "Conexões ativas");
```

### P: Como usar métricas em controllers?

**R:** Exemplo completo:
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

### P: Quais métricas são coletadas automaticamente?

**R:** O pacote coleta automaticamente:
- **Runtime .NET**: GC, threads, exceções, memória
- **ASP.NET Core**: Requisições HTTP, duração, status codes
- **HTTP Client**: Requisições outbound, duração, status codes

### P: Como acessar as métricas?

**R:** As métricas ficam disponíveis em:
- **Endpoint**: `http://localhost:9090/metrics`
- **Formato**: Prometheus
- **Integração**: Prometheus, Grafana, etc.

## 📝 Logs

### P: Como usar logs estruturados?

**R:** Use o `ILogger` injetado:
```csharp
public class MeuServico
{
    private readonly ILogger<MeuServico> _logger;
    
    public MeuServico(ILogger<MeuServico> logger)
    {
        _logger = logger;
    }
    
    public async Task ProcessarItem(string itemId)
    {
        _logger.LogInformation("Processando item {ItemId}", itemId);
        
        try
        {
            await ProcessarDocumento(itemId);
            _logger.LogInformation("Item {ItemId} processado com sucesso", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar item {ItemId}", itemId);
            throw;
        }
    }
}
```

### P: Como configurar logs para diferentes ambientes?

**R:** Use configuração baseada em ambiente:

**Desenvolvimento**:
```json
{
  "Observability": {
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Debug"
  }
}
```

**Produção**:
```json
{
  "Observability": {
    "EnableConsoleLogging": false,
    "LokiUrl": "http://loki:3100",
    "MinimumLogLevel": "Information"
  }
}
```

### P: Como usar Correlation ID?

**R:** O Correlation ID é automático quando `EnableCorrelationId = true`:
```csharp
// O Correlation ID é adicionado automaticamente aos logs
_logger.LogInformation("Processando requisição {RequestId}", requestId);
```

### P: Posso usar outros sinks do Serilog?

**R:** Sim! Configure o Serilog manualmente:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/meuservico-.txt")
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableLogging = true;
    options.EnableConsoleLogging = false; // Usar Serilog customizado
    options.LokiUrl = ""; // Remove Loki
});

builder.Host.UseSerilog();
```

## 🔍 Tracing

### P: Como criar traces customizados?

**R:** Use a classe `ActivitySourceFactory`:
```csharp
using Package.Observability;

public class MeuServico
{
    public async Task ProcessarItem(string itemId)
    {
        using var activity = ActivitySourceFactory.StartActivity("MeuServico", "ProcessarItem");
        
        activity?.SetTag("item.id", itemId);
        activity?.SetTag("item.type", "documento");
        
        try
        {
            await ProcessarDocumento(itemId);
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

### P: Como usar tracing em HTTP clients?

**R:** O tracing é automático para HTTP clients quando `EnableHttpClientInstrumentation = true`:
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
        // O tracing é automático
        var response = await _httpClient.GetAsync(endpoint);
        return await response.Content.ReadAsStringAsync();
    }
}
```

### P: Como configurar diferentes tipos de Activity?

**R:** Use o parâmetro `ActivityKind`:
```csharp
// Client (chamadas HTTP)
using var activity = ActivitySourceFactory.StartActivity("ApiClient", "GetData", ActivityKind.Client);

// Server (processamento de requisições)
using var activity = ActivitySourceFactory.StartActivity("MeuServico", "ProcessarItem", ActivityKind.Server);

// Internal (processamento interno)
using var activity = ActivitySourceFactory.StartActivity("MeuServico", "ProcessarItem", ActivityKind.Internal);
```

## 🐳 Docker

### P: Como usar com Docker?

**R:** Configure as variáveis de ambiente:
```yaml
# docker-compose.yml
services:
  meu-servico:
    build: .
    ports:
      - "5000:80"
      - "9090:9090"
    environment:
      - Observability__ServiceName=MeuServico
      - Observability__EnableMetrics=true
      - Observability__EnableTracing=true
      - Observability__EnableLogging=true
      - Observability__LokiUrl=http://loki:3100
      - Observability__CollectorEndpoint=http://otel-collector:4317
```

### P: Como configurar para desenvolvimento com Docker?

**R:** Use esta configuração:
```yaml
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
```

## 🚨 Troubleshooting

### P: Métricas não aparecem no Prometheus?

**R:** Verifique:
1. `EnableMetrics = true`
2. Porta correta (`PrometheusPort`)
3. Prometheus configurado para fazer scrape
4. Endpoint acessível: `http://localhost:9090/metrics`

### P: Logs não aparecem no Loki?

**R:** Verifique:
1. `EnableLogging = true`
2. `LokiUrl` correto
3. Loki rodando e acessível
4. Configuração de rede

### P: Traces não aparecem no Tempo?

**R:** Verifique:
1. `EnableTracing = true`
2. `OtlpEndpoint` correto
3. Tempo rodando e acessível
4. Configuração de rede

### P: Como debug de configuração?

**R:** Adicione este código:
```csharp
var configuration = builder.Configuration.GetSection("Observability");
var options = configuration.Get<ObservabilityOptions>();

Console.WriteLine($"ServiceName: {options.ServiceName}");
Console.WriteLine($"EnableMetrics: {options.EnableMetrics}");
Console.WriteLine($"EnableTracing: {options.EnableTracing}");
Console.WriteLine($"EnableLogging: {options.EnableLogging}");
Console.WriteLine($"LokiUrl: {options.LokiUrl}");
Console.WriteLine($"OtlpEndpoint: {options.OtlpEndpoint}");
```

### P: Como tratar erros de configuração?

**R:** Use try-catch:
```csharp
try
{
    builder.Services.AddObservability(builder.Configuration);
}
catch (ObservabilityConfigurationException ex)
{
    Console.WriteLine($"Erro de configuração: {ex.Message}");
    
    // Fallback
    builder.Services.AddObservability(options =>
    {
        options.ServiceName = "MeuServico-Fallback";
        options.EnableMetrics = false;
        options.EnableTracing = false;
        options.EnableLogging = true;
        options.EnableConsoleLogging = true;
    });
}
```

## 🔧 Configuração Avançada

### P: Como usar Health Checks?

**R:** Os Health Checks são registrados automaticamente:
```csharp
// Health checks são registrados automaticamente
// Acesse em: http://localhost:5000/health

// Para health checks customizados:
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy("Database OK"))
    .AddCheck("external-api", () => HealthCheckResult.Healthy("External API OK"));

// Mapear endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### P: Como configurar labels customizados?

**R:** Use `AdditionalLabels` e `LokiLabels`:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    
    // Labels para métricas
    options.AdditionalLabels.Add("environment", "production");
    options.AdditionalLabels.Add("version", "1.0.0");
    options.AdditionalLabels.Add("team", "backend");
    
    // Labels para Loki
    options.LokiLabels.Add("app", "meu-servico");
    options.LokiLabels.Add("component", "api");
    options.LokiLabels.Add("tier", "backend");
});
```

### P: Como configurar instrumentação específica?

**R:** Use as opções de instrumentação:
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableRuntimeInstrumentation = true;      // Métricas de runtime .NET
    options.EnableHttpClientInstrumentation = true;   // Instrumentação HTTP Client
    options.EnableAspNetCoreInstrumentation = true;   // Instrumentação ASP.NET Core
});
```

## 📊 Monitoramento

### P: Quais métricas são importantes para monitorar?

**R:** Métricas importantes:
- `http_requests_total` - Total de requisições HTTP
- `http_request_duration_seconds` - Duração das requisições
- `dotnet_gc_collections_total` - Coleta de lixo
- `dotnet_threadpool_threads_total` - Threads do pool
- `dotnet_exceptions_total` - Total de exceções

### P: Como configurar alertas?

**R:** Use Prometheus Alert Rules:
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
```

### P: Como configurar dashboards?

**R:** Use Grafana com as métricas do Prometheus:
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
      }
    ]
  }
}
```

## 🤝 Contribuição

### P: Como contribuir?

**R:** Siga estes passos:
1. Abra uma issue para discussão
2. Faça um fork do repositório
3. Crie uma branch para sua feature
4. Faça commit das mudanças
5. Abra um pull request

### P: Como reportar bugs?

**R:** Abra uma issue com:
1. Descrição do problema
2. Passos para reproduzir
3. Configuração usada
4. Logs de erro
5. Versão do .NET e do pacote

### P: Como sugerir melhorias?

**R:** Abra uma issue com:
1. Descrição da melhoria
2. Caso de uso
3. Benefícios esperados
4. Exemplos de implementação

---

**Ainda tem dúvidas?** Consulte a [documentação completa](README.md) ou abra uma [issue](https://github.com/your-org/Package.Observability/issues) no GitHub.