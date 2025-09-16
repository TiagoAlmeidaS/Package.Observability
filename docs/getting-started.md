# Guia de Início Rápido

Este guia te ajudará a começar a usar o Package.Observability em sua aplicação .NET 8.

## 📦 Instalação

### Via NuGet Package Manager

```bash
Install-Package Package.Observability
```

### Via .NET CLI

```bash
dotnet add package Package.Observability
```

### Via PackageReference

```xml
<PackageReference Include="Package.Observability" Version="1.0.0" />
```

## 🚀 Configuração Básica

### 1. Configure o appsettings.json

```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "LokiUrl": "http://localhost:3100",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### 2. Configure no Program.cs

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Adiciona observabilidade completa
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

// Expor endpoint de métricas Prometheus
app.MapPrometheusScrapingEndpoint();

app.Run();
```

## 🎯 Exemplos por Tipo de Aplicação

### ASP.NET Core Web API

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.Run();
```

### Worker Service

```csharp
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MeuWorker>();
builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();
host.Run();
```

### Console Application

```csharp
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();

// Seu código aqui
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicação iniciada");

await host.RunAsync();
```

## 📊 Verificando se Funcionou

### 1. Verifique os Logs

Após iniciar a aplicação, você deve ver logs estruturados no console:

```
[14:30:15 INF] MinhaAplicacao Aplicação iniciada
[14:30:15 INF] MinhaAplicacao Métricas disponíveis em: http://localhost:9090/metrics
```

### 2. Acesse o Endpoint de Métricas

Para aplicações web, acesse:
- **Métricas Prometheus**: `http://localhost:9090/metrics`

### 3. Verifique as Métricas Básicas

As seguintes métricas devem estar disponíveis:
- `process_runtime_dotnet_gc_heap_size_bytes` - Tamanho do heap GC
- `http_requests_received_total` - Total de requisições HTTP (se aplicável)
- `dotnet_gc_collections_total` - Total de coletas de GC

## 🔧 Configuração Avançada

### Configuração por Código

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.PrometheusPort = 9110;
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.OtlpEndpoint = "http://jaeger:4317";
    options.AdditionalLabels.Add("environment", "production");
    options.LokiLabels.Add("team", "backend");
});
```

### Configuração de Logs

```csharp
// Configuração customizada de logs
builder.Services.AddObservability(options =>
{
    options.EnableConsoleLogging = true;
    options.MinimumLogLevel = "Debug";
    options.EnableCorrelationId = true;
    options.LokiLabels.Add("component", "api");
});
```

## 🐳 Stack de Observabilidade (Opcional)

Para visualizar métricas, logs e traces, use o Docker Compose incluído:

```bash
# Na raiz do projeto
docker-compose up -d
```

Isso iniciará:
- **Prometheus**: http://localhost:9091
- **Grafana**: http://localhost:3000 (admin/admin)
- **Loki**: http://localhost:3100
- **Jaeger**: http://localhost:16686

## 📈 Próximos Passos

1. **Crie Métricas Customizadas**: Veja [Exemplos](examples.md)
2. **Configure Traces Customizados**: Veja [API Reference](api-reference.md)
3. **Configure Dashboards**: Veja [Configuração](configuration.md)
4. **Resolva Problemas**: Veja [Troubleshooting](troubleshooting.md)

## ❓ Problemas Comuns

### Aplicação não inicia

- Verifique se todas as dependências estão instaladas
- Verifique se as URLs do Loki e OTLP estão acessíveis
- Verifique os logs de erro

### Métricas não aparecem

- Verifique se `EnableMetrics` está `true`
- Verifique se o endpoint `/metrics` está mapeado
- Verifique se há requisições sendo feitas à aplicação

### Logs não aparecem no Loki

- Verifique se `LokiUrl` está correto
- Verifique se o Loki está rodando
- Verifique se `EnableLogging` está `true`

## 📚 Recursos Adicionais

- [Configuração Completa](configuration.md)
- [Exemplos Práticos](examples.md)
- [API Reference](api-reference.md)
- [Troubleshooting](troubleshooting.md)
