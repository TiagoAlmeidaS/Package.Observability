# 🚫 Exemplo: Usando Package.Observability SEM Loki

Este exemplo mostra como usar o `Package.Observability` sem Loki, usando apenas console logging ou outros sinks do Serilog.

## 🎯 Cenários Sem Loki

### 1. Desenvolvimento Local
- Apenas logs no console
- Sem necessidade de infraestrutura externa
- Debug fácil e rápido

### 2. Aplicações Simples
- Aplicações que não precisam de agregação de logs
- Logs locais são suficientes
- Redução de complexidade

### 3. Ambientes com Logging Alternativo
- Usando outros sistemas de logging
- Integração com sistemas existentes
- Logs enviados para outros destinos

## 🔧 Configurações

### Configuração 1: Apenas Console (Desenvolvimento)

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico-Dev";
    options.EnableMetrics = true;           // Métricas locais
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = true;           // Logs habilitados
    options.EnableConsoleLogging = true;    // Apenas console
    options.LokiUrl = "";                  // Remove Loki
    options.OtlpEndpoint = "";             // Remove OTLP
    options.MinimumLogLevel = "Debug";     // Logs detalhados
});

var app = builder.Build();
app.Run();
```

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
    "MinimumLogLevel": "Debug",
    "AdditionalLabels": {
      "environment": "development"
    }
  }
}
```

### Configuração 2: Console + Arquivo (Produção Simples)

```csharp
// Program.cs
using Package.Observability;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog manualmente
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/meuservico-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = false;
    options.EnableLogging = true;
    options.EnableConsoleLogging = false;   // Desabilitar console do pacote
    options.LokiUrl = "";                  // Remove Loki
    options.OtlpEndpoint = "";             // Remove OTLP
    options.MinimumLogLevel = "Information";
});

// Usar Serilog configurado manualmente
builder.Host.UseSerilog();

var app = builder.Build();
app.Run();
```

### Configuração 3: Apenas Métricas (Sem Logs)

```csharp
// Program.cs
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;           // Apenas métricas
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = false;          // Sem logs
    options.PrometheusPort = 9090;         // Porta do Prometheus
    options.AdditionalLabels.Add("environment", "production");
});

var app = builder.Build();
app.Run();
```

### Configuração 4: Logs Customizados com Outros Sinks

```csharp
// Program.cs
using Package.Observability;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog com múltiplos sinks
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/meuservico-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .WriteTo.Seq("http://seq:5341")  // Seq para desenvolvimento
    .WriteTo.Elasticsearch("http://elasticsearch:9200")  // Elasticsearch
    .CreateLogger();

builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.EnableConsoleLogging = false;   // Usar Serilog customizado
    options.LokiUrl = "";                  // Remove Loki
    options.CollectorEndpoint = "http://otel-collector:4317";  // Manter tracing
    options.MinimumLogLevel = "Information";
});

builder.Host.UseSerilog();

var app = builder.Build();
app.Run();
```

## 📊 Exemplo Completo: ASP.NET Core Web API

### Projeto: MeuServico

```csharp
// Program.cs
using Package.Observability;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/meuservico-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

// Configurar observabilidade
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = false;          // Sem tracing
    options.EnableLogging = true;
    options.EnableConsoleLogging = false;   // Usar Serilog customizado
    options.LokiUrl = "";                  // Remove Loki
    options.OtlpEndpoint = "";             // Remove OTLP
    options.MinimumLogLevel = "Information";
});

// Outros serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Usar Serilog
builder.Host.UseSerilog();

var app = builder.Build();

// Configurar pipeline
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

### Controller com Métricas e Logs

```csharp
// Controllers/ProdutosController.cs
using Microsoft.AspNetCore.Mvc;
using Package.Observability;
using System.Diagnostics;

namespace MeuServico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ILogger<ProdutosController> _logger;
    
    // Métricas customizadas
    private static readonly Counter<int> _produtoRequests = 
        ObservabilityMetrics.CreateCounter<int>("MeuServico", "produto_requests_total", "count", "Total de requisições de produtos");
    
    private static readonly Histogram<double> _produtoDuration = 
        ObservabilityMetrics.CreateHistogram<double>("MeuServico", "produto_duration_seconds", "seconds", "Duração das requisições de produtos");

    public ProdutosController(ILogger<ProdutosController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetProdutos()
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Iniciando busca de produtos");
        
        try
        {
            // Simular busca de produtos
            var produtos = await BuscarProdutos();
            
            _produtoRequests.Add(1, new KeyValuePair<string, object?>("status", "success"));
            _logger.LogInformation("Produtos encontrados: {Count}", produtos.Count);
            
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _produtoRequests.Add(1, new KeyValuePair<string, object?>("status", "error"));
            _logger.LogError(ex, "Erro ao buscar produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
        finally
        {
            _produtoDuration.Record(stopwatch.Elapsed.TotalSeconds);
        }
    }
    
    private async Task<List<Produto>> BuscarProdutos()
    {
        // Simular delay
        await Task.Delay(100);
        
        return new List<Produto>
        {
            new Produto { Id = 1, Nome = "Produto 1", Preco = 10.50m },
            new Produto { Id = 2, Nome = "Produto 2", Preco = 20.00m }
        };
    }
}

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
```

### appsettings.json

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableMetrics": true,
    "EnableTracing": false,
    "EnableLogging": true,
    "EnableConsoleLogging": false,
    "LokiUrl": "",
    "OtlpEndpoint": "",
    "MinimumLogLevel": "Information",
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/meuservico-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

## 🐳 Docker Compose (Sem Loki)

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
      - ASPNETCORE_ENVIRONMENT=Production
      - Observability__ServiceName=MeuServico
      - Observability__EnableMetrics=true
      - Observability__EnableTracing=false
      - Observability__EnableLogging=true
      - Observability__EnableConsoleLogging=false
      - Observability__LokiUrl=
      - Observability__OtlpEndpoint=
    volumes:
      - ./logs:/app/logs
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

## 📊 Monitoramento

### Métricas Disponíveis

- `http_requests_total` - Total de requisições HTTP
- `http_request_duration_seconds` - Duração das requisições
- `produto_requests_total` - Total de requisições de produtos (customizada)
- `produto_duration_seconds` - Duração das requisições de produtos (customizada)
- `dotnet_gc_collections_total` - Coleta de lixo
- `dotnet_threadpool_threads_total` - Threads do pool

### Logs Estruturados

- Logs no console com formato legível
- Logs em arquivo com rotação diária
- Logs estruturados com propriedades
- Correlation ID automático

### Health Checks

- `http://localhost:5000/health` - Health check geral
- Verificação de componentes de observabilidade
- Status de métricas e logging

## 🎯 Vantagens de Não Usar Loki

### 1. Simplicidade
- Menos infraestrutura para gerenciar
- Configuração mais simples
- Menos pontos de falha

### 2. Performance
- Sem latência de rede para logs
- Logs locais são mais rápidos
- Menos overhead de sistema

### 3. Debug
- Logs diretamente no console
- Fácil visualização durante desenvolvimento
- Sem necessidade de ferramentas externas

### 4. Flexibilidade
- Pode usar outros sinks do Serilog
- Integração com sistemas existentes
- Controle total sobre destino dos logs

## 🚨 Considerações

### 1. Agregação de Logs
- Logs ficam distribuídos entre instâncias
- Dificuldade para análise centralizada
- Necessidade de ferramentas externas para agregação

### 2. Retenção
- Logs locais podem ocupar muito espaço
- Necessidade de rotação e limpeza
- Perda de logs em caso de falha do servidor

### 3. Análise
- Dificuldade para análise de logs distribuídos
- Necessidade de ferramentas de agregação
- Complexidade para alertas baseados em logs

## 🔧 Alternativas ao Loki

### 1. Seq (Desenvolvimento)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();
```

### 2. Elasticsearch (Produção)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Elasticsearch("http://elasticsearch:9200")
    .CreateLogger();
```

### 3. Application Insights (Azure)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights("instrumentation-key")
    .CreateLogger();
```

### 4. CloudWatch (AWS)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.AmazonCloudWatch("region", "log-group")
    .CreateLogger();
```

## 📚 Conclusão

Usar o `Package.Observability` sem Loki é perfeitamente viável e pode ser a escolha certa para:

- **Desenvolvimento local**: Simplicidade e debug fácil
- **Aplicações simples**: Sem necessidade de agregação complexa
- **Ambientes com logging alternativo**: Integração com sistemas existentes
- **Protótipos e MVPs**: Redução de complexidade

O pacote oferece flexibilidade total para escolher apenas os componentes que você precisa, mantendo a simplicidade e eficiência.
