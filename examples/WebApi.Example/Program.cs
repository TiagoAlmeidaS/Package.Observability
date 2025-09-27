using Microsoft.Extensions.Options;
using Package.Observability;
using WebApi.Example.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona observabilidade completa
builder.Services.AddObservability(builder.Configuration);

// Registrar serviços de exemplo
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Registrar serviços com instrumentação automática (ZERO CONFIGURAÇÃO)
builder.Services.AddScoped<IAutoWeatherService, AutoWeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adicionar middleware de telemetria automática (ZERO CONFIGURAÇÃO)
// Similar ao Tempo, Loki e Prometheus - funciona automaticamente
app.UseAutoObservabilityTelemetry();

app.UseAuthorization();
app.MapControllers();

// Expor endpoint de métricas Prometheus apenas se métricas estiverem habilitadas
var observabilityOptions = app.Services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
if (observabilityOptions.EnableMetrics)
{
    app.MapPrometheusScrapingEndpoint();
}

// Expor endpoint de health checks
app.MapHealthChecks("/health");

// Log de inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("WebApi.Example iniciada com observabilidade completa");
logger.LogInformation("Métricas disponíveis em: http://localhost:9090/metrics");
logger.LogInformation("Swagger disponível em: https://localhost:7000/swagger");

app.Run();

public partial class Program { }