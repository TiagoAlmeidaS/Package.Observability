# Upgrade para Tempo + OpenTelemetry Collector

Este documento descreve as mudanças realizadas para integrar o projeto com **Grafana Tempo** e **OpenTelemetry Collector**, substituindo o Jaeger anterior.

## 🚀 O que mudou

### Antes (Jaeger)
- ✅ Grafana
- ✅ Loki  
- ✅ Prometheus
- ❌ Jaeger (substituído)
- ❌ Sem Collector

### Depois (Tempo + Collector)
- ✅ Grafana
- ✅ Loki
- ✅ Prometheus
- ✅ **Tempo** (novo)
- ✅ **OpenTelemetry Collector** (novo)

## 📦 Novas Dependências

### Docker Compose
- `grafana/tempo:latest` - Armazenamento e consulta de traces
- `otel/opentelemetry-collector-contrib:latest` - Coleta e processamento de telemetria

### Configurações
- `tempo.yml` - Configuração do Tempo
- `otel-collector.yml` - Configuração do Collector
- `datasources.yml` - Datasources do Grafana incluindo Tempo

## 🔧 Configurações Atualizadas

### ObservabilityOptions
```csharp
public class ObservabilityOptions
{
    // ... propriedades existentes ...
    
    /// <summary>
    /// Tempo endpoint URL for trace storage and querying
    /// </summary>
    public string TempoEndpoint { get; set; } = "http://localhost:3200";

    /// <summary>
    /// OpenTelemetry Collector endpoint for traces, metrics, and logs
    /// </summary>
    public string CollectorEndpoint { get; set; } = "http://localhost:4317";
}
```

### Docker Compose
```yaml
services:
  tempo:
    image: grafana/tempo:latest
    container_name: tempo
    ports:
      - "3200:3200"    # Tempo HTTP
      - "4317:4317"    # OTLP gRPC receiver
      - "4318:4318"    # OTLP HTTP receiver
    volumes:
      - ./observability/tempo.yml:/etc/tempo.yml
      - tempo-data:/tmp/tempo
    command: -config.file=/etc/tempo.yml

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    container_name: otel-collector
    ports:
      - "8888:8888"    # Prometheus metrics
      - "8889:8889"    # Prometheus exporter
      - "13133:13133"  # Health check
      - "1777:1777"    # pprof
      - "55679:55679"  # zpages
    volumes:
      - ./observability/otel-collector.yml:/etc/otel-collector.yml
    command: ["--config=/etc/otel-collector.yml"]
    depends_on:
      - tempo
      - loki
      - prometheus
```

## 🎯 Benefícios do Upgrade

### 1. **Tempo vs Jaeger**
- **Melhor integração** com Grafana
- **Performance superior** para consultas de traces
- **Correlação automática** entre traces, logs e métricas
- **Interface unificada** no Grafana

### 2. **OpenTelemetry Collector**
- **Processamento centralizado** de telemetria
- **Transformações** de dados antes do envio
- **Múltiplos destinos** (Tempo, Loki, Prometheus)
- **Filtragem e enriquecimento** de dados

### 3. **Stack Completa**
- **Grafana**: Visualização unificada
- **Tempo**: Armazenamento de traces
- **Loki**: Armazenamento de logs
- **Prometheus**: Armazenamento de métricas
- **Collector**: Processamento centralizado

## 🔄 Migração

### 1. **Atualizar Configurações**
```json
{
  "Observability": {
    "ServiceName": "MeuServico",
    "CollectorEndpoint": "http://localhost:4317",
    "TempoEndpoint": "http://localhost:3200",
    "LokiUrl": "http://localhost:3100"
  }
}
```

### 2. **Executar Docker Compose**
```bash
docker-compose up -d
```

### 3. **Verificar Serviços**
- **Grafana**: http://localhost:3000 (admin/admin)
- **Tempo**: http://localhost:3200
- **Loki**: http://localhost:3100
- **Prometheus**: http://localhost:9091
- **Collector**: http://localhost:13133 (health check)

## 📊 Dashboards

### Tempo Dashboard
- **Trace Search**: Busca de traces por critérios
- **Trace Rate**: Taxa de traces por serviço
- **Service Map**: Mapa de dependências
- **Correlação**: Links entre traces, logs e métricas

### Datasources Configurados
- **Prometheus**: Métricas
- **Loki**: Logs
- **Tempo**: Traces

## 🚨 Breaking Changes

### 1. **Endpoints OTLP**
- **Antes**: `http://jaeger:4317`
- **Depois**: `http://otel-collector:4317`

### 2. **Interface de Traces**
- **Antes**: Jaeger UI (http://localhost:16686)
- **Depois**: Grafana + Tempo (http://localhost:3000)

### 3. **Configuração**
- **Novo**: `CollectorEndpoint` e `TempoEndpoint`
- **Mantido**: `OtlpEndpoint` (compatibilidade)

## 🔍 Troubleshooting

### 1. **Collector não inicia**
```bash
# Verificar logs
docker logs otel-collector

# Verificar configuração
docker exec otel-collector cat /etc/otel-collector.yml
```

### 2. **Tempo não recebe traces**
```bash
# Verificar logs
docker logs tempo

# Verificar conectividade
curl http://localhost:3200/api/overrides
```

### 3. **Grafana não conecta ao Tempo**
- Verificar se o plugin `grafana-tempo-datasource` está instalado
- Verificar configuração em `datasources.yml`
- Reiniciar o Grafana

## 📚 Recursos Adicionais

- [Grafana Tempo Documentation](https://grafana.com/docs/tempo/)
- [OpenTelemetry Collector Documentation](https://opentelemetry.io/docs/collector/)
- [Grafana Tempo Datasource](https://grafana.com/grafana/plugins/grafana-tempo-datasource/)

## 🎉 Conclusão

O upgrade para Tempo + OpenTelemetry Collector oferece:

- **Stack completa** de observabilidade
- **Melhor performance** e integração
- **Interface unificada** no Grafana
- **Processamento centralizado** de telemetria
- **Correlação automática** entre dados

A migração é **backward compatible** e mantém todas as funcionalidades existentes.
