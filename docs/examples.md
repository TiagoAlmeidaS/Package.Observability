# 🎯 Exemplos de Uso

Esta seção contém exemplos práticos de como usar o Package.Observability em diferentes cenários.

## 🌐 ASP.NET Core Web API

### Configuração Básica

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

### Controller com Métricas e Tracing

```csharp
using Microsoft.AspNetCore.Mvc;
using Package.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ILogger<ProdutosController> _logger;
    
    // Métricas customizadas
    private static readonly Counter<int> _requestCounter = 
        ObservabilityMetrics.CreateCounter<int>("MeuServico", "produtos_requests_total", "count", "Total de requisições para produtos");
    
    private static readonly Histogram<double> _requestDuration = 
        ObservabilityMetrics.CreateHistogram<double>("MeuServico", "produtos_request_duration", "ms", "Duração das requisições");

    public ProdutosController(ILogger<ProdutosController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetProdutos()
    {
        using var activity = ActivitySourceFactory.StartActivity("MeuServico", "GetProdutos");
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Buscando produtos");
        
        try
        {
            // Simular busca no banco
            await Task.Delay(Random.Shared.Next(50, 200));
            
            var produtos = new[]
            {
                new { Id = 1, Nome = "Produto A", Preco = 29.90 },
                new { Id = 2, Nome = "Produto B", Preco = 49.90 }
            };

            activity?.SetTag("produtos.count", produtos.Length);
            
            _logger.LogInformation("Produtos encontrados: {Count}", produtos.Length);
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));
            
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _requestCounter.Add(1, new KeyValuePair<string, object?>("status", "error"));
            throw;
        }
        finally
        {
            _requestDuration.Record(stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 🔄 Worker Service

### Configuração

```csharp
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ProcessadorWorker>();
builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();
host.Run();
```

### Worker com Observabilidade

```csharp
using Package.Observability;
using System.Diagnostics.Metrics;

public class ProcessadorWorker : BackgroundService
{
    private readonly ILogger<ProcessadorWorker> _logger;
    
    // Métricas específicas do worker
    private static readonly Counter<int> _processedItems = 
        ObservabilityMetrics.CreateCounter<int>("ProcessadorWorker", "items_processed_total", "count", "Total de itens processados");
    
    private static readonly Gauge<int> _queueSize = 
        ObservabilityMetrics.CreateObservableGauge<int>("ProcessadorWorker", "queue_size", () => GetQueueSize(), "count", "Tamanho atual da fila");

    public ProcessadorWorker(ILogger<ProcessadorWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySourceFactory.StartActivity("ProcessadorWorker", "ProcessBatch");
            
            try
            {
                await ProcessarLote(stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
        }
    }

    private async Task ProcessarLote(CancellationToken cancellationToken)
    {
        using var activity = ActivitySourceFactory.StartActivity("ProcessadorWorker", "ProcessarLote");
        
        var items = await ObterItensParaProcessar();
        
        activity?.SetTag("batch.size", items.Count);
        _logger.LogInformation("Processando lote de {Count} itens", items.Count);
        
        foreach (var item in items)
        {
            using var itemActivity = ActivitySourceFactory.StartActivity("ProcessadorWorker", "ProcessarItem");
            itemActivity?.SetTag("item.id", item.Id);
            
            await ProcessarItem(item, cancellationToken);
            _processedItems.Add(1, new KeyValuePair<string, object?>("type", item.Type));
        }
        
        _logger.LogInformation("Lote processado com sucesso");
    }
    
    private static int GetQueueSize() => Random.Shared.Next(0, 100);
}
```

## 🖥️ Console Application

### Aplicação Simples

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Aplicação console iniciada");

using var activity = ActivitySourceFactory.StartActivity("ConsoleApp", "MainOperation");

try
{
    await ExecutarOperacao(logger);
    logger.LogInformation("Operação concluída com sucesso");
}
catch (Exception ex)
{
    logger.LogError(ex, "Erro na operação principal");
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
}

await host.StopAsync();

static async Task ExecutarOperacao(ILogger logger)
{
    using var activity = ActivitySourceFactory.StartActivity("ConsoleApp", "ExecutarOperacao");
    
    logger.LogInformation("Iniciando operação...");
    
    // Simular trabalho
    await Task.Delay(2000);
    
    activity?.SetTag("operation.duration", "2000ms");
    logger.LogInformation("Operação finalizada");
}
```

## 🔧 Serviço Customizado com Observabilidade

### Interface e Implementação

```csharp
public interface IEmailService
{
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    
    // Métricas específicas do serviço
    private static readonly Counter<int> _emailsSent = 
        ObservabilityMetrics.CreateCounter<int>("EmailService", "emails_sent_total", "count", "Total de emails enviados");
    
    private static readonly Histogram<double> _emailSendDuration = 
        ObservabilityMetrics.CreateHistogram<double>("EmailService", "email_send_duration", "ms", "Duração do envio de email");

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        using var activity = ActivitySourceFactory.StartActivity("EmailService", "EnviarEmail");
        var stopwatch = Stopwatch.StartNew();
        
        activity?.SetTag("email.destinatario", destinatario);
        activity?.SetTag("email.assunto", assunto);
        
        _logger.LogInformation("Enviando email para {Destinatario} com assunto {Assunto}", 
            destinatario, assunto);
        
        try
        {
            // Simular envio de email
            await Task.Delay(Random.Shared.Next(500, 2000));
            
            var sucesso = Random.Shared.NextDouble() > 0.1; // 90% de sucesso
            
            if (sucesso)
            {
                _logger.LogInformation("Email enviado com sucesso para {Destinatario}", destinatario);
                _emailsSent.Add(1, new KeyValuePair<string, object?>("status", "success"));
                activity?.SetTag("email.status", "sent");
            }
            else
            {
                _logger.LogWarning("Falha ao enviar email para {Destinatario}", destinatario);
                _emailsSent.Add(1, new KeyValuePair<string, object?>("status", "failed"));
                activity?.SetTag("email.status", "failed");
            }
            
            return sucesso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Destinatario}", destinatario);
            _emailsSent.Add(1, new KeyValuePair<string, object?>("status", "error"));
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _emailSendDuration.Record(stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Registro do Serviço

```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddObservability(builder.Configuration);
```

## 🔍 Middleware Customizado

### Middleware de Correlation ID

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using var activity = ActivitySourceFactory.StartActivity("Middleware", "CorrelationId");
        activity?.SetTag("correlation.id", correlationId);

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            _logger.LogInformation("Processando requisição com Correlation ID: {CorrelationId}", correlationId);
            await _next(context);
        }
    }
}

// Registro
app.UseMiddleware<CorrelationIdMiddleware>();
```

## 📊 Dashboard de Health Check

### Health Check com Métricas

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseHealthCheck> _logger;
    private static readonly Counter<int> _healthChecks = 
        ObservabilityMetrics.CreateCounter<int>("HealthCheck", "database_checks_total", "count", "Total de health checks do banco");

    public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySourceFactory.StartActivity("HealthCheck", "DatabaseCheck");
        
        try
        {
            // Simular verificação do banco
            await Task.Delay(100, cancellationToken);
            
            var isHealthy = Random.Shared.NextDouble() > 0.05; // 95% healthy
            
            if (isHealthy)
            {
                _logger.LogInformation("Database health check passou");
                _healthChecks.Add(1, new KeyValuePair<string, object?>("status", "healthy"));
                activity?.SetTag("health.status", "healthy");
                return HealthCheckResult.Healthy("Database está funcionando");
            }
            else
            {
                _logger.LogWarning("Database health check falhou");
                _healthChecks.Add(1, new KeyValuePair<string, object?>("status", "unhealthy"));
                activity?.SetTag("health.status", "unhealthy");
                return HealthCheckResult.Unhealthy("Database não está respondendo");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no health check do database");
            _healthChecks.Add(1, new KeyValuePair<string, object?>("status", "error"));
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return HealthCheckResult.Unhealthy("Erro ao verificar database", ex);
        }
    }
}

// Registro
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

app.MapHealthChecks("/health");
```

## 🧪 Configuração para Testes

### Helper para Testes

```csharp
public static class TestObservabilityHelper
{
    public static IServiceCollection AddTestObservability(this IServiceCollection services)
    {
        return services.AddObservability(options =>
        {
            options.ServiceName = "TestService";
            options.EnableMetrics = false;
            options.EnableTracing = false;
            options.EnableLogging = true;
            options.EnableConsoleLogging = false;
            options.LokiUrl = "";
            options.OtlpEndpoint = "";
        });
    }
}
```

### Teste de Integração

```csharp
[TestClass]
public class ProdutosControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddTestObservability();
                });
            });
        
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetProdutos_DeveRetornarSucesso()
    {
        // Act
        var response = await _client.GetAsync("/api/produtos");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }
}
```

## 📈 Métricas de Negócio

### Exemplo de E-commerce

```csharp
public class PedidoService
{
    private readonly ILogger<PedidoService> _logger;
    
    // Métricas de negócio
    private static readonly Counter<int> _pedidosCreated = 
        ObservabilityMetrics.CreateCounter<int>("Ecommerce", "pedidos_created_total", "count", "Total de pedidos criados");
    
    private static readonly Histogram<double> _pedidoValue = 
        ObservabilityMetrics.CreateHistogram<double>("Ecommerce", "pedido_value", "BRL", "Valor dos pedidos");
    
    private static readonly Counter<int> _pedidosCanceled = 
        ObservabilityMetrics.CreateCounter<int>("Ecommerce", "pedidos_canceled_total", "count", "Total de pedidos cancelados");

    public async Task<Pedido> CriarPedidoAsync(CriarPedidoRequest request)
    {
        using var activity = ActivitySourceFactory.StartActivity("PedidoService", "CriarPedido");
        
        activity?.SetTag("pedido.cliente_id", request.ClienteId);
        activity?.SetTag("pedido.items_count", request.Items.Count);
        activity?.SetTag("pedido.valor_total", request.ValorTotal);
        
        _logger.LogInformation("Criando pedido para cliente {ClienteId} no valor de {Valor:C}", 
            request.ClienteId, request.ValorTotal);
        
        try
        {
            var pedido = await ProcessarPedido(request);
            
            // Métricas de negócio
            _pedidosCreated.Add(1, 
                new KeyValuePair<string, object?>("categoria", pedido.Categoria),
                new KeyValuePair<string, object?>("canal", pedido.Canal));
            
            _pedidoValue.Record(pedido.ValorTotal, 
                new KeyValuePair<string, object?>("categoria", pedido.Categoria));
            
            _logger.LogInformation("Pedido {PedidoId} criado com sucesso", pedido.Id);
            
            return pedido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido para cliente {ClienteId}", request.ClienteId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

Estes exemplos mostram como integrar observabilidade em diferentes tipos de aplicações .NET 8, desde APIs simples até serviços complexos com métricas de negócio específicas.