namespace WebApi.Example.Services;

/// <summary>
/// Serviço que demonstra instrumentação automática - ZERO CONFIGURAÇÃO
/// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
/// </summary>
public interface IAutoWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days);
    Task<WeatherForecast> GetForecastForDateAsync(DateOnly date);
    Task<bool> ValidateForecastAsync(WeatherForecast forecast);
    Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast);
    Task<WeatherForecast> UpdateForecastAsync(int id, WeatherForecast forecast);
    Task<bool> DeleteForecastAsync(int id);
}

public class AutoWeatherService : IAutoWeatherService
{
    private readonly ILogger<AutoWeatherService> _logger;

    public AutoWeatherService(ILogger<AutoWeatherService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_getforecastasync_calls_total, app_autoweatherservice_getforecastasync_duration_seconds
    /// </summary>
    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Gerando previsão para {Days} dias", days);

        if (days <= 0 || days > 30)
        {
            throw new ArgumentException("Days must be between 1 and 30", nameof(days));
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(100, 300));

        var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        }).ToArray();

        _logger.LogInformation("Previsão gerada para {Days} dias", days);
        return forecasts;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_getforecastfordateasync_calls_total, app_autoweatherservice_getforecastfordateasync_duration_seconds
    /// </summary>
    public async Task<WeatherForecast> GetForecastForDateAsync(DateOnly date)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Buscando previsão para data: {Date}", date);

        if (date < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Cannot get forecast for past dates", nameof(date));
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(50, 150));

        var forecast = new WeatherForecast
        {
            Date = date,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        };

        _logger.LogInformation("Previsão encontrada para data: {Date}", date);
        return forecast;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_validateforecastasync_calls_total, app_autoweatherservice_validateforecastasync_duration_seconds
    /// </summary>
    public async Task<bool> ValidateForecastAsync(WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Validando previsão para data: {Date}", forecast.Date);

        if (forecast == null)
        {
            throw new ArgumentNullException(nameof(forecast));
        }

        // Simular validação
        await Task.Delay(Random.Shared.Next(10, 50));

        var isValid = forecast.TemperatureC >= -50 && forecast.TemperatureC <= 60 && 
                     !string.IsNullOrEmpty(forecast.Summary);

        _logger.LogInformation("Validação concluída para data: {Date} - Válida: {IsValid}", 
            forecast.Date, isValid);

        return isValid;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_createforecastasync_calls_total, app_autoweatherservice_createforecastasync_duration_seconds
    /// </summary>
    public async Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Criando previsão para data: {Date}", forecast.Date);

        if (forecast == null)
        {
            throw new ArgumentNullException(nameof(forecast));
        }

        // Simular validação
        await Task.Delay(Random.Shared.Next(20, 80));

        // Simular falha ocasional
        if (Random.Shared.NextDouble() < 0.05) // 5% de chance de falha
        {
            throw new InvalidOperationException("Erro simulado na criação da previsão");
        }

        _logger.LogInformation("Previsão criada para data: {Date}", forecast.Date);
        return forecast;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_updateforecastasync_calls_total, app_autoweatherservice_updateforecastasync_duration_seconds
    /// </summary>
    public async Task<WeatherForecast> UpdateForecastAsync(int id, WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Atualizando previsão ID: {Id} para data: {Date}", id, forecast.Date);

        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than zero", nameof(id));
        }

        if (forecast == null)
        {
            throw new ArgumentNullException(nameof(forecast));
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(40, 120));

        _logger.LogInformation("Previsão ID: {Id} atualizada para data: {Date}", id, forecast.Date);
        return forecast;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente
    /// Métricas: app_autoweatherservice_deleteforecastasync_calls_total, app_autoweatherservice_deleteforecastasync_duration_seconds
    /// </summary>
    public async Task<bool> DeleteForecastAsync(int id)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Excluindo previsão ID: {Id}", id);

        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than zero", nameof(id));
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(20, 60));

        _logger.LogInformation("Previsão ID: {Id} excluída", id);
        return true;
    }

    private static string GetRandomSummary()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        return summaries[Random.Shared.Next(summaries.Length)];
    }
}
