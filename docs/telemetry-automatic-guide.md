# Guia de Telemetria Automática - Similar ao Application Insights

Este guia demonstra como usar a telemetria automática do Package.Observability, que funciona de forma similar ao Application Insights, mas usando OpenTelemetry.

## 🚀 Visão Geral

A telemetria automática permite capturar métricas, logs e traces automaticamente sem código manual, similar ao Application Insights. Inclui:

- **Middleware Automático** - Captura métricas de requisições HTTP automaticamente
- **Atributos de Telemetria** - Marca métodos para instrumentação automática
- **Serviço de Telemetria** - API para captura manual de métricas personalizadas
- **Interceptação Automática** - Captura telemetria de métodos marcados

## 📋 Configuração Básica

### 1. Registrar Serviços

```csharp
// Program.cs
builder.Services.AddObservability(builder.Configuration);
```

### 2. Adicionar Middleware

```csharp
// Program.cs
app.UseObservabilityTelemetry(); // Adiciona captura automática de requisições HTTP
```

## 🔧 Uso com Atributos (Recomendado)

### Método com Telemetria Automática

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    [HttpGet]
    [Telemetry(
        MetricName = "weather_requests",
        Description = "Total weather requests",
        Type = MetricType.Counter,
        CaptureDuration = true,
        Tags = new[] { "endpoint", "weather" }
    )]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // A telemetria é capturada automaticamente
        await Task.Delay(100);
        return GenerateForecast();
    }
}
```

### Configurações do Atributo

```csharp
[Telemetry(
    MetricName = "custom_metric_name",           // Nome da métrica
    Description = "Description of the metric",   // Descrição
    Type = MetricType.Histogram,                // Tipo: Counter, Histogram, Gauge
    Unit = "seconds",                           // Unidade da métrica
    CaptureDuration = true,                     // Capturar duração
    CaptureExceptions = true,                   // Capturar exceções
    Tags = new[] { "tag1", "value1", "tag2", "value2" } // Tags personalizadas
)]
```

## 🛠️ Uso Manual com ITelemetryService

### Injeção de Dependência

```csharp
public class WeatherService
{
    private readonly ITelemetryService _telemetryService;

    public WeatherService(ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }
}
```

### Métricas Personalizadas

```csharp
public async Task<WeatherForecast> GetForecastAsync(DateOnly date)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        // Registrar evento
        _telemetryService.RecordEvent("forecast_request_started", new Dictionary<string, object>
        {
            ["date"] = date.ToString(),
            ["user_id"] = GetCurrentUserId()
        });

        var forecast = await GenerateForecastAsync(date);

        // Registrar contador
        _telemetryService.RecordCounter("forecast_requests", 1, new Dictionary<string, object>
        {
            ["success"] = "true",
            ["date"] = date.ToString()
        });

        // Registrar histograma
        _telemetryService.RecordHistogram("forecast_duration", stopwatch.Elapsed.TotalSeconds, "seconds", new Dictionary<string, object>
        {
            ["date"] = date.ToString()
        });

        return forecast;
    }
    catch (Exception ex)
    {
        // Registrar erro
        _telemetryService.RecordCounter("forecast_errors", 1, new Dictionary<string, object>
        {
            ["error_type"] = ex.GetType().Name,
            ["date"] = date.ToString()
        });
        throw;
    }
}
```

### Dependências Externas

```csharp
public async Task<WeatherData> CallExternalApiAsync(string city)
{
    var stopwatch = Stopwatch.StartNew();
    var success = true;
    var errorMessage = (string?)null;

    try
    {
        var response = await _httpClient.GetAsync($"https://api.weather.com/{city}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
    catch (Exception ex)
    {
        success = false;
        errorMessage = ex.Message;
        throw;
    }
    finally
    {
        // Registrar dependência externa
        _telemetryService.RecordDependency(
            "external-weather-api",
            "http",
            success,
            stopwatch.Elapsed,
            new Dictionary<string, object>
            {
                ["city"] = city,
                ["error"] = errorMessage ?? "none"
            });
    }
}
```

## 📊 Tipos de Métricas Suportadas

### 1. Counter (Contador)
```csharp
_telemetryService.RecordCounter("requests_total", 1, tags);
```

### 2. Histogram (Histograma)
```csharp
_telemetryService.RecordHistogram("request_duration", 1.5, "seconds", tags);
```

### 3. Gauge (Medidor)
```csharp
_telemetryService.RecordGauge("active_connections", 42, "connections", tags);
```

### 4. Event (Evento)
```csharp
_telemetryService.RecordEvent("user_login", new Dictionary<string, object>
{
    ["user_id"] = "123",
    ["login_method"] = "oauth"
});
```

## 🔄 Middleware Automático

O middleware captura automaticamente:

- **Contadores de Requisições** - `http_requests_total`
- **Duração de Requisições** - `http_request_duration_seconds`
- **Contadores de Erro** - `http_requests_errors_total`
- **Logs de Requisições** - Início, conclusão e erros
- **Traces** - Via OpenTelemetry

### Tags Automáticas

```csharp
// Tags adicionadas automaticamente pelo middleware
{
    "method": "GET",
    "path": "/weather",
    "status_code": "200",
    "service": "MyService"
}
```

## 🎯 Exemplo Completo

### Controller com Telemetria Automática

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    public WeatherController(ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }

    [HttpGet]
    [Telemetry(
        MetricName = "weather_forecast_requests",
        Description = "Total weather forecast requests",
        Type = MetricType.Counter,
        CaptureDuration = true
    )]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // Telemetria automática via atributo
        await Task.Delay(100);
        return GenerateForecast();
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateForecast([FromBody] WeatherForecast forecast)
    {
        // Telemetria manual para casos complexos
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var isValid = await ValidateAsync(forecast);
            
            _telemetryService.RecordCounter("forecast_validations", 1, new Dictionary<string, object>
            {
                ["valid"] = isValid.ToString(),
                ["temperature"] = forecast.TemperatureC.ToString()
            });

            return Ok(new { valid = isValid });
        }
        catch (Exception ex)
        {
            _telemetryService.RecordCounter("forecast_validation_errors", 1, new Dictionary<string, object>
            {
                ["error_type"] = ex.GetType().Name
            });
            throw;
        }
        finally
        {
            _telemetryService.RecordHistogram("forecast_validation_duration", 
                stopwatch.Elapsed.TotalSeconds, "seconds");
        }
    }
}
```

## 🔧 Configuração Avançada

### Configuração no appsettings.json

```json
{
  "Observability": {
    "ServiceName": "WeatherService",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "AdditionalLabels": {
      "environment": "production",
      "version": "1.0.0",
      "team": "platform"
    }
  }
}
```

### Configuração por Código

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "WeatherService";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.AdditionalLabels.Add("environment", "production");
    options.AdditionalLabels.Add("version", "1.0.0");
});
```

## 📈 Visualização no Grafana

As métricas são automaticamente expostas no endpoint `/metrics` e podem ser visualizadas no Grafana:

- **http_requests_total** - Total de requisições HTTP
- **http_request_duration_seconds** - Duração das requisições
- **http_requests_errors_total** - Total de erros HTTP
- **Métricas personalizadas** - Definidas via atributos ou ITelemetryService

## 🚀 Vantagens sobre Application Insights

1. **Open Source** - Sem custos de licenciamento
2. **OpenTelemetry** - Padrão da indústria
3. **Flexibilidade** - Controle total sobre coleta e exportação
4. **Performance** - Menor overhead
5. **Customização** - Fácil de estender e personalizar

## 🔍 Debugging e Troubleshooting

### Verificar se Telemetria está Funcionando

```csharp
// Verificar se métricas estão habilitadas
if (ObservabilityMetrics.IsMetricsEnabled)
{
    // Métricas estão ativas
}

// Verificar logs de telemetria
_logger.LogInformation("Telemetry service is active");
```

### Logs de Debug

```csharp
// Habilitar logs detalhados
builder.Logging.AddFilter("Package.Observability", LogLevel.Debug);
```

## 📚 Próximos Passos

1. **Implementar em Produção** - Começar com middleware automático
2. **Adicionar Métricas Personalizadas** - Usar ITelemetryService
3. **Configurar Dashboards** - Criar visualizações no Grafana
4. **Monitorar Performance** - Acompanhar métricas de duração
5. **Alertas** - Configurar alertas baseados em métricas

---

**Nota**: Esta implementação oferece funcionalidade similar ao Application Insights, mas com maior controle e flexibilidade usando OpenTelemetry e Prometheus.
