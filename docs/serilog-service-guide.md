# Guia do SerilogService - Package.Observability

## Visão Geral

O `SerilogService` é um serviço de telemetria dedicado que estende as capacidades de logging do Package.Observability, fornecendo uma interface rica e configurável para logging estruturado com Serilog.

## Características Principais

- **Logging Estruturado**: Suporte completo a logging estruturado com Serilog
- **Múltiplos Sinks**: Console, arquivo, Loki, Seq, Elasticsearch
- **Configuração Flexível**: Configuração via `ObservabilityOptions`
- **Health Checks**: Monitoramento de saúde do serviço
- **Enrichers**: Enriquecimento automático de logs com contexto
- **Correlation ID**: Rastreamento de requisições entre serviços

## Configuração

### 1. Configuração Básica no appsettings.json

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "EnableLogging": true,
    "EnableConsoleLogging": true,
    "MinimumLogLevel": "Information",
    "EnableCorrelationId": true,
    
    // Configurações específicas do Serilog
    "EnableFileLogging": true,
    "FileLoggingPath": "Logs/meuservico-.log",
    "ConsoleOutputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}",
    "FileOutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {ServiceName} {Message:lj}{NewLine}{Exception}",
    
    // Loki
    "LokiUrl": "http://localhost:3100",
    "LokiLabels": {
      "app": "meuservico",
      "component": "api"
    },
    
    // Seq (opcional)
    "EnableSeqLogging": true,
    "SeqUrl": "http://localhost:5341",
    
    // Propriedades customizadas
    "CustomProperties": {
      "version": "1.0.0",
      "environment": "production"
    }
  }
}
```

### 2. Configuração Programática

```csharp
builder.Services.AddObservability(builder.Configuration, configureOptions: options =>
{
    options.ServiceName = "MeuServico";
    options.EnableLogging = true;
    options.EnableFileLogging = true;
    options.EnableSeqLogging = true;
    options.SeqUrl = "http://localhost:5341";
    options.CustomProperties.Add("version", "1.0.0");
});
```

## Uso do SerilogService

### 1. Injeção de Dependência

```csharp
public class MeuController : ControllerBase
{
    private readonly SerilogService _serilogService;
    private readonly ILogger<MeuController> _logger;

    public MeuController(SerilogService serilogService, ILogger<MeuController> logger)
    {
        _serilogService = serilogService;
        _logger = logger;
    }
}
```

### 2. Logging Estruturado

```csharp
[HttpPost]
public async Task<IActionResult> ProcessarDados([FromBody] DadosRequest request)
{
    // Log estruturado com SerilogService
    _serilogService.Log(LogEventLevel.Information, 
        "Processando dados para usuário {UserId} com {DataCount} itens", 
        request.UserId, request.Items.Count);

    try
    {
        // Processamento...
        var resultado = await ProcessarAsync(request);
        
        _serilogService.Log(LogEventLevel.Information, 
            "Dados processados com sucesso para usuário {UserId}, resultado: {Result}", 
            request.UserId, resultado);
            
        return Ok(resultado);
    }
    catch (Exception ex)
    {
        // Log de exceção estruturado
        _serilogService.LogException(LogEventLevel.Error, ex, 
            "Erro ao processar dados para usuário {UserId}", 
            request.UserId);
            
        return StatusCode(500, "Erro interno");
    }
}
```

### 3. Logger com Contexto Adicional

```csharp
public async Task ProcessarItem(Item item)
{
    // Criar logger com contexto específico
    var scopedLogger = _serilogService.CreateScopedLogger(new Dictionary<string, object>
    {
        ["ItemId"] = item.Id,
        ["ItemType"] = item.Type,
        ["ProcessId"] = Guid.NewGuid().ToString()
    });

    scopedLogger.LogInformation("Iniciando processamento do item");
    
    // O contexto será automaticamente incluído em todos os logs
    scopedLogger.LogInformation("Item processado com sucesso");
}
```

### 4. Verificação de Status

```csharp
public IActionResult GetLoggingStatus()
{
    var status = _serilogService.GetConfigurationStatus();
    
    return Ok(new
    {
        IsConfigured = status.IsConfigured,
        SinkCount = status.SinkCount,
        IsLokiEnabled = status.IsLokiEnabled,
        MinimumLogLevel = status.MinimumLogLevel
    });
}
```

## Health Checks

O SerilogService inclui health checks específicos:

### Endpoint de Health Check

```bash
GET /health
```

### Health Check Específico do Serilog

```bash
GET /health/serilog
```

### Exemplo de Resposta

```json
{
  "status": "Healthy",
  "description": "SerilogService funcionando corretamente",
  "data": {
    "IsConfigured": true,
    "SinkCount": 3,
    "IsLokiEnabled": true,
    "MinimumLogLevel": "Information",
    "ServiceName": "MeuServico"
  }
}
```

## Configurações Avançadas

### 1. Múltiplos Sinks

```json
{
  "Observability": {
    "EnableConsoleLogging": true,
    "EnableFileLogging": true,
    "EnableSeqLogging": true,
    "EnableElasticsearchLogging": true,
    "LokiUrl": "http://localhost:3100",
    "SeqUrl": "http://localhost:5341",
    "ElasticsearchUrl": "http://localhost:9200"
  }
}
```

### 2. Templates Customizados

```json
{
  "Observability": {
    "ConsoleOutputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} [{SourceContext}] {Message:lj}{NewLine}{Exception}",
    "FileOutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {ServiceName} [{SourceContext}] {Message:lj}{NewLine}{Exception}"
  }
}
```

### 3. Enrichers Adicionais

```json
{
  "Observability": {
    "AdditionalEnrichers": [
      "Serilog.Enrichers.Environment",
      "Serilog.Enrichers.Process",
      "Serilog.Enrichers.Thread"
    ]
  }
}
```

## Integração com Middleware

### Request Logging

```csharp
// No Program.cs
if (app.Services.GetRequiredService<IOptions<ObservabilityOptions>>().Value.EnableRequestLogging)
{
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => 
        {
            var slowRequestThreshold = app.Services.GetRequiredService<IOptions<ObservabilityOptions>>()
                .Value.SlowRequestThreshold ?? 1000;
                
            if (ex != null) return LogEventLevel.Error;
            if (elapsed > slowRequestThreshold) return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
    });
}
```

## Monitoramento e Troubleshooting

### 1. Verificar Status do Serviço

```csharp
var serilogService = serviceProvider.GetRequiredService<SerilogService>();
var status = serilogService.GetConfigurationStatus();

Console.WriteLine($"Serilog configurado: {status.IsConfigured}");
Console.WriteLine($"Número de sinks: {status.SinkCount}");
Console.WriteLine($"Loki habilitado: {status.IsLokiEnabled}");
```

### 2. Logs de Debug

```csharp
// Habilitar logs de debug para troubleshooting
builder.Logging.AddFilter("Package.Observability", LogLevel.Debug);
```

### 3. Health Check Detalhado

```bash
# Verificar health check específico do Serilog
curl http://localhost:5000/health/serilog
```

## Exemplos Práticos

### 1. API Controller com Logging Estruturado

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly SerilogService _serilogService;
    private readonly IProdutoService _produtoService;

    public ProdutosController(SerilogService serilogService, IProdutoService produtoService)
    {
        _serilogService = serilogService;
        _produtoService = produtoService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduto(int id)
    {
        _serilogService.Log(LogEventLevel.Information, 
            "Buscando produto {ProdutoId} para usuário {UserId}", 
            id, User.Identity?.Name);

        try
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                _serilogService.Log(LogEventLevel.Warning, 
                    "Produto {ProdutoId} não encontrado", id);
                return NotFound();
            }

            _serilogService.Log(LogEventLevel.Information, 
                "Produto {ProdutoId} encontrado: {ProdutoNome}", 
                id, produto.Nome);

            return Ok(produto);
        }
        catch (Exception ex)
        {
            _serilogService.LogException(LogEventLevel.Error, ex, 
                "Erro ao buscar produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno");
        }
    }
}
```

### 2. Background Service com Logging

```csharp
public class ProcessamentoBackgroundService : BackgroundService
{
    private readonly SerilogService _serilogService;
    private readonly ILogger<ProcessamentoBackgroundService> _logger;

    public ProcessamentoBackgroundService(SerilogService serilogService, ILogger<ProcessamentoBackgroundService> logger)
    {
        _serilogService = serilogService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _serilogService.Log(LogEventLevel.Information, 
                    "Iniciando processamento em lote {Timestamp}", DateTime.UtcNow);

                await ProcessarLoteAsync();

                _serilogService.Log(LogEventLevel.Information, 
                    "Processamento em lote concluído {Timestamp}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _serilogService.LogException(LogEventLevel.Error, ex, 
                    "Erro no processamento em lote");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Considerações de Performance

1. **Async Logging**: O SerilogService é otimizado para logging assíncrono
2. **Batching**: Logs são enviados em lotes para melhor performance
3. **Memory Usage**: Use `Flush()` quando necessário para liberar memória
4. **Sink Selection**: Configure apenas os sinks necessários para sua aplicação

## Segurança

1. **Dados Sensíveis**: Nunca logue senhas, tokens ou dados pessoais
2. **Sanitização**: Use mascaramento para dados sensíveis
3. **Retenção**: Configure políticas de retenção de logs
4. **Acesso**: Restrinja acesso aos endpoints de health check em produção

## Conclusão

O SerilogService fornece uma solução completa e flexível para logging estruturado no Package.Observability, seguindo os padrões estabelecidos e oferecendo integração perfeita com o ecossistema de observabilidade.
