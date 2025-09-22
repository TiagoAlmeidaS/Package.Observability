using Microsoft.AspNetCore.Mvc;

namespace WebApi.Example.Controllers;

/// <summary>
/// Controller que demonstra instrumentação automática - ZERO CONFIGURAÇÃO
/// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AutoWeatherController : ControllerBase
{
    private readonly ILogger<AutoWeatherController> _logger;

    public AutoWeatherController(ILogger<AutoWeatherController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente
    /// Métricas: app_autoweathercontroller_get_calls_total, app_autoweathercontroller_get_duration_seconds
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Gerando previsão do tempo automaticamente");

        // Simular processamento
        await Task.Delay(Random.Shared.Next(50, 200));

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        _logger.LogInformation("Previsão gerada automaticamente: {Count} dias", forecasts.Length);

        return forecasts;
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente
    /// Métricas: app_autoweathercontroller_getbyid_calls_total, app_autoweathercontroller_getbyid_duration_seconds
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WeatherForecast>> GetById(int id)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Buscando previsão para ID: {Id}", id);

        if (id <= 0)
        {
            return BadRequest("ID deve ser maior que zero");
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(30, 100));

        var forecast = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(id)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };

        _logger.LogInformation("Previsão encontrada para ID: {Id}", id);

        return Ok(forecast);
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente
    /// Métricas: app_autoweathercontroller_create_calls_total, app_autoweathercontroller_create_duration_seconds
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WeatherForecast>> Create([FromBody] WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Criando nova previsão para data: {Date}", forecast.Date);

        if (forecast == null)
        {
            return BadRequest("Dados da previsão são obrigatórios");
        }

        // Simular validação
        await Task.Delay(Random.Shared.Next(20, 80));

        // Simular falha ocasional
        if (Random.Shared.NextDouble() < 0.1) // 10% de chance de falha
        {
            throw new InvalidOperationException("Erro simulado na criação da previsão");
        }

        _logger.LogInformation("Previsão criada com sucesso para data: {Date}", forecast.Date);

        return CreatedAtAction(nameof(GetById), new { id = 1 }, forecast);
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente
    /// Métricas: app_autoweathercontroller_update_calls_total, app_autoweathercontroller_update_duration_seconds
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WeatherForecast>> Update(int id, [FromBody] WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Atualizando previsão ID: {Id} para data: {Date}", id, forecast.Date);

        if (id <= 0)
        {
            return BadRequest("ID deve ser maior que zero");
        }

        if (forecast == null)
        {
            return BadRequest("Dados da previsão são obrigatórios");
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(40, 120));

        _logger.LogInformation("Previsão ID: {Id} atualizada com sucesso", id);

        return Ok(forecast);
    }

    /// <summary>
    /// Endpoint que é instrumentado automaticamente
    /// Métricas: app_autoweathercontroller_delete_calls_total, app_autoweathercontroller_delete_duration_seconds
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Excluindo previsão ID: {Id}", id);

        if (id <= 0)
        {
            return BadRequest("ID deve ser maior que zero");
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(20, 60));

        _logger.LogInformation("Previsão ID: {Id} excluída com sucesso", id);

        return NoContent();
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
}
