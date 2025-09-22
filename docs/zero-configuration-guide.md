# Guia de Instrumentação Automática - ZERO CONFIGURAÇÃO

Este guia demonstra como usar a instrumentação automática do Package.Observability, que funciona de forma completamente automática, similar ao Tempo, Loki e Prometheus.

## 🚀 Visão Geral

A instrumentação automática funciona **SEM NENHUMA CONFIGURAÇÃO MANUAL**:

- **Auto-Discovery** - Descobre automaticamente aplicações, serviços e endpoints
- **Zero Configuration** - Funciona automaticamente sem código manual
- **Reflection-based** - Usa reflection para instrumentação automática
- **Convention-based** - Baseado em convenções, não configuração

## 📋 Configuração Mínima

### 1. Registrar Serviços (Automático)

```csharp
// Program.cs
builder.Services.AddObservability(builder.Configuration);
```

### 2. Adicionar Middleware (Automático)

```csharp
// Program.cs
app.UseAutoObservabilityTelemetry(); // ZERO CONFIGURAÇÃO
```

**Isso é tudo!** A instrumentação funciona automaticamente.

## 🔧 Uso com Atributos (Opcional)

### Marcar Classes para Instrumentação Automática

```csharp
[ApiController]
[Route("[controller]")]
[AutoInstrument] // Marca para instrumentação automática
public class WeatherController : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // A telemetria é capturada automaticamente
        // Métricas: app_weathercontroller_get_calls_total, app_weathercontroller_get_duration_seconds
        return await GenerateForecast();
    }
}
```

### Marcar Serviços para Instrumentação Automática

```csharp
[AutoInstrument] // Marca para instrumentação automática
public class WeatherService
{
    public async Task<WeatherForecast> GetForecastAsync(DateOnly date)
    {
        // A telemetria é capturada automaticamente
        // Métricas: app_weatherservice_getforecastasync_calls_total, app_weatherservice_getforecastasync_duration_seconds
        return await GenerateForecastAsync(date);
    }
}
```

## 🎯 Exemplo Completo - ZERO CONFIGURAÇÃO

### Controller com Instrumentação Automática

```csharp
[ApiController]
[Route("api/[controller]")]
[AutoInstrument] // Marca para instrumentação automática
public class AutoWeatherController : ControllerBase
{
    private readonly ILogger<AutoWeatherController> _logger;

    public AutoWeatherController(ILogger<AutoWeatherController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Gerando previsão do tempo automaticamente");

        await Task.Delay(Random.Shared.Next(50, 200));

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        return forecasts;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WeatherForecast>> GetById(int id)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Buscando previsão para ID: {Id}", id);

        if (id <= 0)
        {
            return BadRequest("ID deve ser maior que zero");
        }

        await Task.Delay(Random.Shared.Next(30, 100));

        var forecast = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(id)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };

        return Ok(forecast);
    }
}
```

### Serviço com Instrumentação Automática

```csharp
[AutoInstrument] // Marca para instrumentação automática
public class AutoWeatherService
{
    private readonly ILogger<AutoWeatherService> _logger;

    public AutoWeatherService(ILogger<AutoWeatherService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Gerando previsão para {Days} dias", days);

        if (days <= 0 || days > 30)
        {
            throw new ArgumentException("Days must be between 1 and 30", nameof(days));
        }

        await Task.Delay(Random.Shared.Next(100, 300));

        var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        }).ToArray();

        return forecasts;
    }

    public async Task<WeatherForecast> GetForecastForDateAsync(DateOnly date)
    {
        // A telemetria é capturada automaticamente - ZERO CONFIGURAÇÃO
        _logger.LogInformation("Buscando previsão para data: {Date}", date);

        if (date < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Cannot get forecast for past dates", nameof(date));
        }

        await Task.Delay(Random.Shared.Next(50, 150));

        var forecast = new WeatherForecast
        {
            Date = date,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = GetRandomSummary()
        };

        return forecast;
    }
}
```

## 📊 Métricas Capturadas Automaticamente

### Métricas de Sistema (Automáticas)

- **system_cpu_usage_percent** - Uso de CPU
- **system_memory_usage_bytes** - Uso de memória
- **system_gc_collections_total** - Coletas de GC
- **system_thread_count** - Número de threads

### Métricas de Aplicação (Automáticas)

- **app_requests_total** - Total de requisições HTTP
- **app_request_duration_seconds** - Duração das requisições HTTP
- **app_active_connections** - Conexões ativas
- **app_errors_total** - Total de erros

### Métricas de Métodos (Automáticas)

- **app_{classname}_{methodname}_calls_total** - Total de chamadas do método
- **app_{classname}_{methodname}_duration_seconds** - Duração do método

### Exemplos de Métricas Automáticas

```
app_autoweathercontroller_get_calls_total{class="AutoWeatherController",method="Get",success="true"}
app_autoweathercontroller_get_duration_seconds{class="AutoWeatherController",method="Get",success="true"}
app_autoweatherservice_getforecastasync_calls_total{class="AutoWeatherService",method="GetForecastAsync",success="true"}
app_autoweatherservice_getforecastasync_duration_seconds{class="AutoWeatherService",method="GetForecastAsync",success="true"}
```

## 🔍 Descoberta Automática

O sistema descobre automaticamente:

### Aplicações
- **Assemblies** - Todos os assemblies da aplicação
- **Controllers** - Classes que terminam com "Controller"
- **Services** - Classes que terminam com "Service", "Manager", "Handler"

### Endpoints
- **Health Checks** - `/health`, `/health/ready`, `/health/live`
- **Métricas** - `/metrics`, `/prometheus`, `/stats`
- **APIs** - Endpoints de controllers automaticamente

### Métodos
- **Públicos** - Todos os métodos públicos
- **Instância** - Métodos de instância (não estáticos)
- **Não especiais** - Exclui construtores e propriedades

## 🚀 Vantagens sobre Configuração Manual

1. **✅ Zero Configuração** - Funciona automaticamente
2. **✅ Zero Código** - Sem código manual de telemetria
3. **✅ Zero Manutenção** - Não precisa atualizar código
4. **✅ Zero Conhecimento** - Não precisa saber OpenTelemetry
5. **✅ Zero Dependências** - Funciona com qualquer código
6. **✅ Zero Overhead** - Performance otimizada

## 🔧 Configuração Avançada (Opcional)

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

### Configuração por Código (Opcional)

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

- **Métricas de Sistema** - CPU, memória, GC, threads
- **Métricas de Aplicação** - Requisições, duração, erros
- **Métricas de Métodos** - Chamadas e duração por método
- **Métricas de Endpoints** - Requisições HTTP por endpoint

## 🔍 Debugging e Troubleshooting

### Verificar se Instrumentação está Funcionando

```csharp
// Verificar se métricas estão habilitadas
if (ObservabilityMetrics.IsMetricsEnabled)
{
    // Métricas estão ativas
}

// Verificar logs de instrumentação
_logger.LogInformation("Auto-instrumentation is active");
```

### Logs de Debug

```csharp
// Habilitar logs detalhados
builder.Logging.AddFilter("Package.Observability", LogLevel.Debug);
```

## 🎯 Comparação com Tempo, Loki e Prometheus

| Recurso | Tempo | Loki | Prometheus | Package.Observability |
|---------|-------|------|------------|----------------------|
| **Configuração** | Zero | Zero | Zero | Zero |
| **Descoberta** | Automática | Automática | Automática | Automática |
| **Instrumentação** | Automática | Automática | Automática | Automática |
| **Métricas** | Automáticas | Automáticas | Automáticas | Automáticas |
| **Logs** | Automáticos | Automáticos | Automáticos | Automáticos |
| **Traces** | Automáticos | Automáticos | Automáticos | Automáticos |

## 📚 Próximos Passos

1. **Implementar em Produção** - Começar com middleware automático
2. **Adicionar Atributos** - Marcar classes para instrumentação
3. **Configurar Dashboards** - Criar visualizações no Grafana
4. **Monitorar Performance** - Acompanhar métricas automaticamente
5. **Alertas** - Configurar alertas baseados em métricas

---

**Nota**: Esta implementação oferece funcionalidade similar ao Tempo, Loki e Prometheus, mas com **ZERO CONFIGURAÇÃO** necessária. Funciona automaticamente sem código manual!
