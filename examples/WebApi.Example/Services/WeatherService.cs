namespace WebApi.Example.Services;

/// <summary>
/// Serviço de exemplo que demonstra telemetria automática - ZERO CONFIGURAÇÃO
/// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
/// </summary>
public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days);
    Task<WeatherForecast> GetForecastForDateAsync(DateOnly date);
    Task<bool> ValidateForecastAsync(WeatherForecast forecast);
}

public class WeatherService : IWeatherService
{
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: app_weatherservice_getforecastasync_calls_total, app_weatherservice_getforecastasync_duration_seconds
    /// </summary>
    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Gerando previsão do tempo para {Days} dias", days);

        // Simular processamento
        await Task.Delay(Random.Shared.Next(100, 300));

        var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        }).ToArray();

        _logger.LogInformation("Previsão gerada com sucesso para {Days} dias", days);
        return forecasts;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: app_weatherservice_getforecastfordateasync_calls_total, app_weatherservice_getforecastfordateasync_duration_seconds
    /// </summary>
    public async Task<WeatherForecast> GetForecastForDateAsync(DateOnly date)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Buscando previsão para a data {Date}", date);

        // Simular validação de data
        if (date < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Não é possível obter previsão para datas passadas");
        }

        // Simular processamento
        await Task.Delay(Random.Shared.Next(50, 150));

        var forecast = new WeatherForecast
        {
            Date = date,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        };

        _logger.LogInformation("Previsão para {Date} gerada com sucesso", date);
        return forecast;
    }

    /// <summary>
    /// Método que é instrumentado automaticamente - ZERO CONFIGURAÇÃO
    /// Métricas: app_weatherservice_validateforecastasync_calls_total, app_weatherservice_validateforecastasync_duration_seconds
    /// </summary>
    public async Task<bool> ValidateForecastAsync(WeatherForecast forecast)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Validando previsão para {Date}", forecast.Date);

        // Simular validação
        await Task.Delay(Random.Shared.Next(10, 50));

        var isValid = forecast.TemperatureC >= -50 && forecast.TemperatureC <= 60 && 
                     !string.IsNullOrEmpty(forecast.Summary);

        _logger.LogInformation("Validação para {Date} concluída: {IsValid}", forecast.Date, isValid);
        return isValid;
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

