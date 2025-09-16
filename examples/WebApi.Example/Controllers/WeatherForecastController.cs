using Microsoft.AspNetCore.Mvc;
using Package.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WebApi.Example.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    
           // Métricas customizadas (criadas condicionalmente)
           private Counter<int>? _requestCounter;
           private Histogram<double>? _requestDuration;
           private bool _metricsInitialized = false;
           private readonly object _metricsLock = new object();
           private Meter? _meter;

           public WeatherForecastController(ILogger<WeatherForecastController> logger)
           {
               _logger = logger;
           }

           public void Dispose()
           {
               _meter?.Dispose();
           }

           private void InitializeMetricsIfNeeded()
           {
               if (_metricsInitialized) return;

               lock (_metricsLock)
               {
                   if (_metricsInitialized) return;

                   try
                   {
                       // Debug: verificar se métricas estão habilitadas
                       Debug.WriteLine($"InitializeMetricsIfNeeded - ObservabilityMetrics.IsMetricsEnabled: {ObservabilityMetrics.IsMetricsEnabled}");
                       
                       // Verificar se métricas estão habilitadas antes de criar
                       if (!ObservabilityMetrics.IsMetricsEnabled)
                       {
                           Debug.WriteLine("InitializeMetricsIfNeeded - Métricas desabilitadas, não criando métricas");
                           _metricsInitialized = true;
                           return;
                       }

                       // Criar um Meter específico para esta instância
                       _meter = new Meter("WebApi.Example", "1.0.0");
                       _requestCounter = _meter.CreateCounter<int>("weather_requests_total", "count", "Total de requisições para weather");
                       _requestDuration = _meter.CreateHistogram<double>("weather_request_duration", "ms", "Duração das requisições weather");
                       _metricsInitialized = true;
                       Debug.WriteLine("InitializeMetricsIfNeeded - Métricas criadas com sucesso");
                   }
                   catch (Exception ex)
                   {
                       // Log other unexpected errors during metric initialization
                       Debug.WriteLine($"InitializeMetricsIfNeeded - Error initializing metrics: {ex.Message}");
                       _metricsInitialized = true; // Prevent repeated attempts
                   }
               }
           }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // Inicializar métricas se necessário
        InitializeMetricsIfNeeded();
        
        // Criar trace customizado
        using var activity = ActivitySourceFactory.StartActivity("WebApi.Example", "GetWeatherForecast");
        
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Iniciando busca de previsão do tempo");
        
        try
        {
            // Simular algum processamento
            await Task.Delay(Random.Shared.Next(50, 200));
            
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Adicionar informações ao trace
            activity?.SetTag("forecast.count", forecasts.Length);
            activity?.SetTag("forecast.days", 5);
            
            _logger.LogInformation("Previsão do tempo gerada com sucesso. {Count} dias retornados", forecasts.Length);
            
            // Incrementar contador de sucesso (se métricas estão habilitadas)
            _requestCounter?.Add(1, new KeyValuePair<string, object?>("status", "success"));
            
            return forecasts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar previsão do tempo");
            
            // Marcar trace como erro
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Incrementar contador de erro (se métricas estão habilitadas)
            _requestCounter?.Add(1, new KeyValuePair<string, object?>("status", "error"));
            
            throw;
        }
        finally
        {
            // Registrar duração (se métricas estão habilitadas)
            _requestDuration?.Record(stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("Requisição processada em {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
    }

    [HttpGet("slow")]
    public async Task<IActionResult> GetSlow()
    {
        using var activity = ActivitySourceFactory.StartActivity("WebApi.Example", "GetSlow");
        
        _logger.LogInformation("Iniciando operação lenta");
        
        // Simular operação lenta
        await Task.Delay(2000);
        
        activity?.SetTag("operation.type", "slow");
        activity?.SetTag("delay.ms", 2000);
        
        _logger.LogInformation("Operação lenta concluída");
        
        return Ok(new { message = "Operação lenta concluída", timestamp = DateTime.UtcNow });
    }

    [HttpGet("error")]
    public IActionResult GetError()
    {
        // Inicializar métricas se necessário
        InitializeMetricsIfNeeded();
        
        using var activity = ActivitySourceFactory.StartActivity("WebApi.Example", "GetError");
        
        _logger.LogWarning("Endpoint de erro chamado intencionalmente");
        
        try
        {
            throw new InvalidOperationException("Erro simulado para demonstração");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro simulado capturado");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Incrementar contador de erro (se métricas estão habilitadas)
            _requestCounter?.Add(1, new KeyValuePair<string, object?>("status", "error"));
            
            return StatusCode(500, new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
}