# WebApi.Example - Exemplo de Uso do Package.Observability

Este projeto demonstra como usar o `Package.Observability` em uma Web API ASP.NET Core.

## 🚀 Como Executar

1. **Pré-requisitos**
   - .NET 8 SDK
   - Docker (opcional, para stack de observabilidade)

2. **Executar a aplicação**
   ```bash
   cd examples/WebApi.Example
   dotnet run
   ```

3. **Acessar endpoints**
   - Swagger UI: https://localhost:7000/swagger
   - Métricas Prometheus: http://localhost:9090/metrics
   - API: https://localhost:7000/WeatherForecast

## 📊 Endpoints Disponíveis

### GET /WeatherForecast
Retorna previsão do tempo com métricas e tracing automáticos.

**Exemplo de resposta:**
```json
[
  {
    "date": "2024-01-02",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Warm"
  }
]
```

### GET /WeatherForecast/slow
Simula uma operação lenta (2 segundos) para demonstrar traces de longa duração.

### GET /WeatherForecast/error
Simula um erro para demonstrar como erros são capturados em logs e traces.

## 🔍 Observabilidade Implementada

### Métricas Customizadas
- `weather_requests_total`: Contador de requisições com label de status
- `weather_request_duration`: Histograma de duração das requisições

### Logs Estruturados
- Logs de início e fim de operações
- Logs de erro com stack trace
- Correlation ID automático
- Enrichers de processo e thread

### Traces Distribuídos
- Traces customizados para cada endpoint
- Tags informativos (count, duration, etc.)
- Status de erro em casos de exceção
- Instrumentação automática do ASP.NET Core

## 🐳 Stack de Observabilidade (Opcional)

Para visualizar as métricas, logs e traces, execute:

```bash
# Na raiz do projeto
docker-compose up -d
```

Isso iniciará:
- **Prometheus**: http://localhost:9091
- **Grafana**: http://localhost:3000 (admin/admin)
- **Loki**: http://localhost:3100
- **Tempo**: http://localhost:3200

## 📈 Visualizando os Dados

### Prometheus
Acesse http://localhost:9091 e consulte:
- `weather_requests_total`
- `weather_request_duration`
- `http_requests_received_total`
- `dotnet_gc_collections_total`

### Grafana
1. Acesse http://localhost:3000
2. Login: admin/admin
3. Adicione data sources:
   - Prometheus: http://prometheus:9090
   - Loki: http://loki:3100
4. Importe dashboards ou crie os seus próprios

### Tempo
Acesse http://localhost:3200 para visualizar traces distribuídos:
- Selecione o serviço "WebApi.Example"
- Visualize traces de requisições
- Analise spans e tags customizados

## 🧪 Testando

Execute algumas requisições para gerar dados:

```bash
# Requisições normais
curl https://localhost:7000/WeatherForecast

# Requisição lenta
curl https://localhost:7000/WeatherForecast/slow

# Requisição com erro
curl https://localhost:7000/WeatherForecast/error
```

## 📝 Configuração

A configuração está em `appsettings.json`:

```json
{
  "Observability": {
    "ServiceName": "WebApi.Example",
    "PrometheusPort": 9090,
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true,
    "LokiUrl": "http://localhost:3100",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

## 🔧 Customização

Para adicionar suas próprias métricas e traces:

```csharp
// Métricas customizadas
private static readonly Counter<int> _myCounter = 
    ObservabilityMetrics.CreateCounter<int>("MeuServico", "my_metric", "count", "Descrição");

// Traces customizados
using var activity = ActivitySourceFactory.StartActivity("MeuServico", "MinhaOperacao");
activity?.SetTag("custom.tag", "valor");
```