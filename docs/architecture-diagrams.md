# Diagramas de Arquitetura de Observabilidade

## 🏗️ Arquitetura Atual (Híbrida)

```
┌─────────────────────────────────────────────────────────────────┐
│                    Aplicação .NET                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │     Logs        │  │    Métricas     │  │     Traces      │ │
│  │   (Serilog)     │  │  (Prometheus)   │  │  (OpenTelemetry)│ │
│  └─────────┬───────┘  └─────────┬───────┘  └─────────┬───────┘ │
└────────────┼────────────────────┼────────────────────┼─────────┘
             │                    │                    │
             ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│     Loki        │  │   Prometheus    │  │ OTLP Collector  │
│  (Agregação)    │  │   (Scraping)    │  │   (Roteamento)  │
└─────────────────┘  └─────────────────┘  └─────────┬───────┘
                                                    │
                                                    ▼
                                          ┌─────────────────┐
                                          │     Tempo       │
                                          │  (Armazenamento)│
                                          └─────────────────┘
```

## 🔄 Arquitetura OTLP Pura

```
┌─────────────────────────────────────────────────────────────────┐
│                    Aplicação .NET                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │     Logs        │  │    Métricas     │  │     Traces      │ │
│  │ (OpenTelemetry) │  │ (OpenTelemetry) │  │ (OpenTelemetry) │ │
│  └─────────┬───────┘  └─────────┬───────┘  └─────────┬───────┘ │
└────────────┼────────────────────┼────────────────────┼─────────┘
             │                    │                    │
             └────────────────────┼────────────────────┘
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │    OTLP Collector       │
                    │  ┌─────────────────┐   │
                    │  │   Processamento │   │
                    │  │   (Filtros,     │   │
                    │  │    Transform.)  │   │
                    │  └─────────────────┘   │
                    └─────────┬───────────────┘
                              │
                    ┌─────────┼───────────────┐
                    │         │               │
                    ▼         ▼               ▼
            ┌───────────┐ ┌───────────┐ ┌───────────┐
            │   Loki    │ │Prometheus │ │   Tempo   │
            │(Logs)     │ │(Métricas) │ │(Traces)   │
            └───────────┘ └───────────┘ └───────────┘
```

## 🎯 Arquitetura Recomendada (Flexível)

```
┌─────────────────────────────────────────────────────────────────┐
│                    Aplicação .NET                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │     Logs        │  │    Métricas     │  │     Traces      │ │
│  │   (Serilog)     │  │  (Prometheus)   │  │  (OpenTelemetry)│ │
│  └─────────┬───────┘  └─────────┬───────┘  └─────────┬───────┘ │
└────────────┼────────────────────┼────────────────────┼─────────┘
             │                    │                    │
             ▼                    ▼                    ▼
    ┌───────────────┐    ┌───────────────┐    ┌───────────────┐
    │   Console     │    │   Prometheus  │    │ OTLP Collector │
    │   + Loki      │    │   (Direto)    │    │   (Sempre)    │
    │   (Desenv.)   │    │               │    └─────────┬───────┘
    └───────────────┘    └───────────────┘              │
                                                        ▼
                                              ┌───────────────┐
                                              │     Tempo     │
                                              │ (Produção)    │
                                              └───────────────┘
```

## 🔧 Configuração por Ambiente

### **Desenvolvimento Local**

```
Aplicação .NET
├── Logs → Console + Loki (direto)
├── Métricas → Prometheus (direto)
└── Traces → OTLP Collector → Tempo
```

**Configuração:**
```csharp
options.EnableConsoleLogging = true;
options.LokiUrl = "http://localhost:3100";
options.EnableMetrics = true;
options.OtlpEndpoint = "http://localhost:4317";
```

### **Produção**

```
Aplicação .NET
└── Tudo → OTLP Collector → {Loki, Prometheus, Tempo}
```

**Configuração:**
```csharp
options.EnableConsoleLogging = false;
options.LokiUrl = "";
options.EnableMetrics = false;
options.OtlpEndpoint = "http://otel-collector:4317";
```

## 📊 Fluxo de Dados Detalhado

### **1. Logs (Serilog + Loki)**

```
Aplicação .NET
    ↓ (ILogger)
Serilog
    ↓ (WriteTo.GrafanaLoki)
Loki
    ↓ (Query)
Grafana
```

### **2. Métricas (Prometheus)**

```
Aplicação .NET
    ↓ (Meter/Counter/Histogram)
OpenTelemetry Metrics
    ↓ (AddPrometheusExporter)
Prometheus Endpoint (/metrics)
    ↓ (Scraping)
Prometheus Server
    ↓ (Query)
Grafana
```

### **3. Traces (OpenTelemetry + Tempo)**

```
Aplicação .NET
    ↓ (ActivitySource)
OpenTelemetry Tracing
    ↓ (AddOtlpExporter)
OTLP Collector
    ↓ (otlp/tempo exporter)
Tempo
    ↓ (Query)
Grafana
```

## 🚀 Vantagens de Cada Abordagem

### **Configuração Direta**

```
✅ Simplicidade
✅ Performance
✅ Controle total
✅ Debugging fácil
❌ Múltiplos pontos de configuração
❌ Menos flexibilidade
```

### **OTLP Pura**

```
✅ Padrão da indústria
✅ Centralização
✅ Flexibilidade máxima
✅ Processamento centralizado
❌ Complexidade
❌ Dependência do Collector
```

### **Híbrida (Recomendada)**

```
✅ Flexibilidade máxima
✅ Performance otimizada
✅ Compatibilidade
✅ Migração gradual
❌ Configuração mais complexa
❌ Múltiplas estratégias
```

## 🔍 Decisão de Arquitetura

### **Use Configuração Direta quando:**
- Desenvolvimento local
- Aplicações simples
- Performance é crítica
- Controle total necessário

### **Use OTLP Pura quando:**
- Produção enterprise
- Múltiplos destinos
- Processamento centralizado
- Ambientes cloud-native

### **Use Híbrida quando:**
- Flexibilidade máxima necessária
- Diferentes ambientes
- Migração gradual
- Casos de uso diversos

A arquitetura híbrida atual do Package.Observability oferece **o melhor dos dois mundos**, permitindo que você escolha a estratégia mais adequada para cada cenário específico.
