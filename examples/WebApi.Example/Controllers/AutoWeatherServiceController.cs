using Microsoft.AspNetCore.Mvc;
using WebApi.Example.Services;

namespace WebApi.Example.Controllers;

/// <summary>
/// Controller que demonstra o uso de serviços com instrumentação automática
/// ZERO CONFIGURAÇÃO - funciona automaticamente
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AutoWeatherServiceController : ControllerBase
{
    private readonly IAutoWeatherService _weatherService;
    private readonly ILogger<AutoWeatherServiceController> _logger;

    public AutoWeatherServiceController(
        IAutoWeatherService weatherService,
        ILogger<AutoWeatherServiceController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_getforecast_calls_total, app_autoweatherservicecontroller_getforecast_duration_seconds
    /// </summary>
    [HttpGet("forecast/{days}")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetForecast(int days)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Solicitando previsão para {Days} dias", days);

        try
        {
            var forecasts = await _weatherService.GetForecastAsync(days);
            return Ok(forecasts);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Parâmetro inválido: {Days} dias", days);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter previsão para {Days} dias", days);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_getforecastbydate_calls_total, app_autoweatherservicecontroller_getforecastbydate_duration_seconds
    /// </summary>
    [HttpGet("forecast/date/{date}")]
    public async Task<ActionResult<WeatherForecast>> GetForecastByDate(DateOnly date)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Solicitando previsão para data: {Date}", date);

        try
        {
            var forecast = await _weatherService.GetForecastForDateAsync(date);
            return Ok(forecast);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Data inválida: {Date}", date);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter previsão para data: {Date}", date);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_validateforecast_calls_total, app_autoweatherservicecontroller_validateforecast_duration_seconds
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateForecast([FromBody] WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Validando previsão para data: {Date}", forecast?.Date);

        if (forecast == null)
        {
            return BadRequest("Dados da previsão são obrigatórios");
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

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_createforecast_calls_total, app_autoweatherservicecontroller_createforecast_duration_seconds
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<WeatherForecast>> CreateForecast([FromBody] WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Criando previsão para data: {Date}", forecast?.Date);

        if (forecast == null)
        {
            return BadRequest("Dados da previsão são obrigatórios");
        }

        try
        {
            var createdForecast = await _weatherService.CreateForecastAsync(forecast);
            return CreatedAtAction(nameof(GetForecastByDate), new { date = createdForecast.Date }, createdForecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar previsão");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_updateforecast_calls_total, app_autoweatherservicecontroller_updateforecast_duration_seconds
    /// </summary>
    [HttpPut("update/{id}")]
    public async Task<ActionResult<WeatherForecast>> UpdateForecast(int id, [FromBody] WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Atualizando previsão ID: {Id} para data: {Date}", id, forecast?.Date);

        if (forecast == null)
        {
            return BadRequest("Dados da previsão são obrigatórios");
        }

        try
        {
            var updatedForecast = await _weatherService.UpdateForecastAsync(id, forecast);
            return Ok(updatedForecast);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "ID inválido: {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar previsão ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint que usa serviço com instrumentação automática
    /// Métricas: app_autoweatherservicecontroller_deleteforecast_calls_total, app_autoweatherservicecontroller_deleteforecast_duration_seconds
    /// </summary>
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> DeleteForecast(int id)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Excluindo previsão ID: {Id}", id);

        try
        {
            var deleted = await _weatherService.DeleteForecastAsync(id);
            if (deleted)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "ID inválido: {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir previsão ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
