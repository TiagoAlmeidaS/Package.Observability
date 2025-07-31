# 🚀 Guia de Início Rápido

Este guia irá te ajudar a configurar observabilidade completa em sua aplicação .NET 8 em poucos minutos.

## 📋 Pré-requisitos

- .NET 8 SDK
- Projeto ASP.NET Core, Worker Service ou Console Application
- (Opcional) Docker para stack de observabilidade

## 🔧 Passo a Passo

### 1. Instalar o Pacote

```bash
dotnet add package Package.Observability
```

### 2. Configurar appsettings.json

Adicione a seção de configuração no seu `appsettings.json`:

```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "PrometheusPort": 9090,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "LokiUrl": "http://localhost:3100",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### 3. Configurar no Program.cs

#### ASP.NET Core Web API

```csharp
using Package.Observability;

var builder = WebApplication.CreateBuilder(args);

// Serviços da aplicação
builder.Services.AddControllers();

// 🎯 Adicionar observabilidade
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

// Pipeline da aplicação
app.UseRouting();
app.MapControllers();

app.Run();
```

#### Worker Service

```csharp
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

// Registrar seu worker
builder.Services.AddHostedService<MeuWorker>();

// 🎯 Adicionar observabilidade
builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();
host.Run();
```

#### Console Application

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Package.Observability;

var builder = Host.CreateApplicationBuilder(args);

// 🎯 Adicionar observabilidade
builder.Services.AddObservability(builder.Configuration);

var host = builder.Build();

// Seu código aqui
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicação iniciada com observabilidade");

await host.RunAsync();
```

### 4. Executar a Aplicação

```bash
dotnet run
```

## ✅ Verificar se Está Funcionando

### Métricas
Acesse: `http://localhost:9090/metrics`

Você deve ver métricas como:
```
# HELP dotnet_gc_collections_total Number of garbage collections
dotnet_gc_collections_total{generation="0"} 5
dotnet_gc_collections_total{generation="1"} 2
dotnet_gc_collections_total{generation="2"} 1

# HELP http_requests_received_total Number of HTTP requests received
http_requests_received_total{method="GET",status_code="200"} 10
```

### Logs
No console, você verá logs estruturados:
```
[14:30:15 INF] MeuServico Aplicação iniciada com observabilidade
[14:30:16 INF] MeuServico Processando requisição GET /api/dados
```

### Traces
Se configurado com Jaeger, os traces aparecerão automaticamente.

## 🐳 Stack de Observabilidade (Opcional)

Para visualizar métricas, logs e traces, execute a stack completa:

```bash
# Baixar configurações
curl -O https://raw.githubusercontent.com/your-org/Package.Observability/main/docker-compose.yml

# Iniciar stack
docker-compose up -d
```

Isso iniciará:
- **Prometheus**: http://localhost:9091
- **Grafana**: http://localhost:3000 (admin/admin)
- **Loki**: http://localhost:3100
- **Jaeger**: http://localhost:16686

## 🎯 Próximos Passos

1. **[Configuração Detalhada](configuration.md)** - Personalizar configurações
2. **[Métricas](metrics.md)** - Criar métricas customizadas
3. **[Logs](logging.md)** - Usar logs estruturados
4. **[Tracing](tracing.md)** - Implementar traces customizados
5. **[Exemplos](examples.md)** - Ver exemplos práticos

## ❗ Problemas Comuns

### Porta 9090 em uso
```json
{
  "Observability": {
    "PrometheusPort": 9091
  }
}
```

### Loki não disponível
```json
{
  "Observability": {
    "EnableLogging": true,
    "EnableConsoleLogging": true,
    "LokiUrl": ""
  }
}
```

### Desabilitar temporariamente
```json
{
  "Observability": {
    "EnableMetrics": false,
    "EnableTracing": false,
    "EnableLogging": true
  }
}
```

## 🆘 Precisa de Ajuda?

- [Troubleshooting](troubleshooting.md)
- [FAQ](faq.md)
- [Issues no GitHub](https://github.com/your-org/Package.Observability/issues)