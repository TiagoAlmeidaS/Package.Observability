using Microsoft.AspNetCore.Mvc;
using WebApi.Example.Services;

namespace WebApi.Example.Controllers;

/// <summary>
/// Controller que demonstra o uso de serviços com telemetria automática
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherServiceController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherServiceController> _logger;

    public WeatherServiceController(
        IWeatherService weatherService,
        ILogger<WeatherServiceController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que usa o serviço com telemetria automática
    /// </summary>
    [HttpGet("forecast/{days}")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetForecast(int days)
    {
        if (days <= 0 || days > 30)
        {
            return BadRequest("Days must be between 1 and 30");
        }

        try
        {
            var forecasts = await _weatherService.GetForecastAsync(days);
            return Ok(forecasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter previsão para {Days} dias", days);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que busca previsão para uma data específica
    /// </summary>
    [HttpGet("forecast/date/{date}")]
    public async Task<ActionResult<WeatherForecast>> GetForecastForDate(DateOnly date)
    {
        try
        {
            var forecast = await _weatherService.GetForecastForDateAsync(date);
            return Ok(forecast);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Data inválida fornecida: {Date}", date);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter previsão para {Date}", date);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que valida uma previsão
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateForecast([FromBody] WeatherForecast forecast)
    {
        if (forecast == null)
        {
            return BadRequest("Forecast data is required");
        }

        try
        {
            var isValid = await _weatherService.ValidateForecastAsync(forecast);
            return Ok(new { valid = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar previsão");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
