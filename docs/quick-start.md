# 🚀 Quick Start - Package.Observability

Este guia rápido mostra como começar a usar o `Package.Observability` em diferentes cenários.

## ⚡ Configuração em 30 Segundos

### 1. Instalar o Pacote

```bash
dotnet add package Package.Observability
```

### 2. Configuração Mínima

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Uma linha para configurar tudo!
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();
app.Run();
```

### 3. Configuração no appsettings.json

```json
{
  "Observability": {
    "ServiceName": "MeuServico"
  }
}
```

**Pronto!** Agora você tem:
- ✅ Métricas em `http://localhost:9090/metrics`
- ✅ Logs estruturados no console
- ✅ Health checks em `/health`

## 🎯 Cenários Comuns

### Desenvolvimento Local (Sem Infraestrutura)

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
});
```

### Apenas Logs (Sem Métricas e Tracing)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;          // Sem métricas
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = true;           // Apenas logs
    options.EnableConsoleLogging = true;    // Console habilitado
});
```

### Apenas Métricas (Sem Logs e Tracing)

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;           // Apenas métricas
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = false;          // Sem logs
    options.PrometheusPort = 9090;         // Porta do Prometheus
});
```

### Produção Completa

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.CollectorEndpoint = "http://otel-collector:4317";
    options.EnableConsoleLogging = false;   // Sem console em produção
});
```

## 📊 Usando Métricas Customizadas

```csharp
using Package.Observability;

public class MeuController : ControllerBase
{
    // Criar métricas
    private static readonly Counter<int> _requestCounter = 
        ObservabilityMetrics.CreateCounter<int>("MeuServico", "requests_total", "count", "Total de requisições");
    
    private static readonly Histogram<double> _requestDuration = 
        ObservabilityMetrics.CreateHistogram<double>("MeuServico", "request_duration_seconds", "seconds", "Duração das requisições");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Seu código aqui
            var result = await ProcessarRequisicao();
            
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            throw;
        }
        finally
        {
            _requestDuration.Record(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
```

## 📝 Usando Logs Estruturados

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
            // Seu código aqui
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

## 🔍 Usando Tracing

```csharp
using Package.Observability;

public class MeuServico
{
    public async Task ProcessarItem(string itemId)
    {
        // Criar activity para tracing
        using var activity = ActivitySourceFactory.StartActivity("MeuServico", "ProcessarItem");
        
        // Adicionar tags
        activity?.SetTag("item.id", itemId);
        
        try
        {
            // Seu código aqui
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

## 🐳 Docker Compose Rápido

```yaml
# docker-compose.yml
version: '3.8'

services:
  meu-servico:
    build: .
    ports:
      - "5000:80"
      - "9090:9090"
    environment:
      - Observability__ServiceName=MeuServico
      - Observability__EnableMetrics=true
      - Observability__EnableTracing=false
      - Observability__EnableLogging=true
      - Observability__EnableConsoleLogging=true

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9091:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
```

## 🔧 Configuração por Ambiente

### appsettings.Development.json

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

### appsettings.Production.json

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

## 🚨 Tratamento de Erros

```csharp
try
{
    builder.Services.AddObservability(builder.Configuration);
}
catch (ObservabilityConfigurationException ex)
{
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
```

## 📊 Endpoints Disponíveis

Após configurar, você terá acesso a:

- **Métricas**: `http://localhost:9090/metrics`
- **Health Check**: `http://localhost:5000/health`
- **Grafana**: `http://localhost:3000` (se configurado)
- **Prometheus**: `http://localhost:9091` (se configurado)

## 🎯 Próximos Passos

1. **Configuração Avançada**: Consulte o [Guia de Uso Completo](usage-guide.md)
2. **Exemplos Detalhados**: Veja [Exemplos de Configuração](configuration-examples.md)
3. **Monitoramento**: Configure alertas e dashboards
4. **Produção**: Implemente health checks e fallbacks

## ❓ Dúvidas Frequentes

**P: Posso usar apenas logs sem Loki?**
R: Sim! Configure `LokiUrl = ""` e `EnableConsoleLogging = true`.

**P: Posso desabilitar métricas?**
R: Sim! Configure `EnableMetrics = false`.

**P: Posso usar apenas tracing?**
R: Sim! Configure `EnableMetrics = false` e `EnableLogging = false`.

**P: Como configurar para desenvolvimento?**
R: Use `EnableConsoleLogging = true` e deixe `LokiUrl` e `OtlpEndpoint` vazios.

**P: Como configurar para produção?**
R: Use `EnableConsoleLogging = false` e configure `LokiUrl` e `OtlpEndpoint`.

---

**Pronto para começar?** Escolha um cenário acima e comece a usar o `Package.Observability` em seus projetos! 🚀
