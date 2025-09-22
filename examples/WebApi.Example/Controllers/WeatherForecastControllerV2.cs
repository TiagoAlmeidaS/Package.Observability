using Microsoft.AspNetCore.Mvc;

namespace WebApi.Example.Controllers;

/// <summary>
/// Controller de exemplo usando telemetria automática - ZERO CONFIGURAÇÃO
/// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastControllerV2 : ControllerBase
{
    private readonly ILogger<WeatherForecastControllerV2> _logger;

    public WeatherForecastControllerV2(ILogger<WeatherForecastControllerV2> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: http_requests_total, http_request_duration_seconds
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Iniciando busca de previsão do tempo");

        // Simular processamento
        await Task.Delay(Random.Shared.Next(50, 200));

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        _logger.LogInformation("Previsão do tempo gerada com sucesso. {Count} dias retornados", forecasts.Length);

        return forecasts;
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: http_requests_total, http_request_duration_seconds
    /// </summary>
    [HttpGet("manual")]
    public async Task<IEnumerable<WeatherForecast>> GetManual()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Iniciando busca manual de previsão do tempo");

        // Simular chamada para API externa
        await SimulateExternalApiCall();

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        _logger.LogInformation("Previsão manual gerada com sucesso. {Count} dias retornados", forecasts.Length);

        return forecasts;
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: http_requests_total, http_request_duration_seconds
    /// </summary>
    [HttpGet("external")]
    public async Task<IActionResult> GetExternal()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Chamando API externa de previsão do tempo");

        // Simular chamada para API externa
        await SimulateExternalApiCall();

        _logger.LogInformation("API externa chamada com sucesso");
        return Ok(new { message = "Dados obtidos da API externa com sucesso" });
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: http_requests_total, http_request_duration_seconds
    /// </summary>
    [HttpGet("metrics-demo")]
    public IActionResult GetMetricsDemo()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Demonstração de métricas automáticas");

        return Ok(new { message = "Métricas automáticas funcionando - ZERO CONFIGURAÇÃO" });
    }

    private async Task SimulateExternalApiCall()
    {
        // Simular latência de rede
        await Task.Delay(Random.Shared.Next(100, 500));

        // Simular falha ocasional
        if (Random.Shared.NextDouble() < 0.1) // 10% de chance de falha
        {
            throw new HttpRequestException("API externa temporariamente indisponível");
        }
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
}
