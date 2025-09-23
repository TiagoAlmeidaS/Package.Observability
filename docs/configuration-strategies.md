# Estratégias de Configuração de Observabilidade

## 🎯 Visão Geral

Este documento explica as diferentes estratégias de configuração disponíveis no Package.Observability e quando usar cada uma.

## 🏗️ Estratégias Disponíveis

### 1. **Configuração Direta (Direct)**

Envia dados diretamente para cada ferramenta sem passar pelo OTLP Collector.

```csharp
builder.Services.AddObservability(options =>
{
    // Logs diretos para Loki
    options.LokiUrl = "http://localhost:3100";
    options.EnableConsoleLogging = true;
    
    // Métricas diretas para Prometheus
    options.EnableMetrics = true;
    options.PrometheusPort = 9090;
    
    // Traces via OTLP (sempre recomendado)
    options.EnableTracing = true;
    options.OtlpEndpoint = "http://localhost:4317";
});
```

**Quando usar:**
- ✅ Desenvolvimento local
- ✅ Aplicações simples
- ✅ Quando você quer controle total
- ✅ Quando performance é crítica

### 2. **Configuração OTLP Pura (OTLP)**

Tudo via OTLP Collector, que roteia para os destinos finais.

```csharp
builder.Services.AddObservability(options =>
{
    // Desabilitar configurações diretas
    options.LokiUrl = "";
    options.EnableConsoleLogging = false;
    options.EnableMetrics = false;
    
    // Apenas OTLP
    options.EnableTracing = true;
    options.EnableLogging = true; // Via OTLP
    options.OtlpEndpoint = "http://otel-collector:4317";
});
```

**Quando usar:**
- ✅ Produção enterprise
- ✅ Múltiplos destinos
- ✅ Processamento centralizado
- ✅ Ambientes cloud-native

### 3. **Configuração Híbrida (Hybrid) - Recomendada**

Combina configurações diretas com OTLP baseado no contexto.

```csharp
builder.Services.AddObservability(options =>
{
    if (environment.IsProduction())
    {
        // Produção: OTLP para centralização
        options.LokiUrl = "";
        options.EnableConsoleLogging = false;
        options.OtlpEndpoint = "http://otel-collector:4317";
    }
    else
    {
        // Desenvolvimento: Configurações diretas
        options.LokiUrl = "http://localhost:3100";
        options.EnableConsoleLogging = true;
        options.OtlpEndpoint = "http://localhost:4317";
    }
});
```

**Quando usar:**
- ✅ Maior flexibilidade
- ✅ Diferentes ambientes
- ✅ Migração gradual
- ✅ Casos de uso diversos

## 🔧 Configuração por Ambiente

### **Desenvolvimento Local**

```json
{
  "Observability": {
    "ServiceName": "MinhaApp",
    "EnableConsoleLogging": true,
    "LokiUrl": "http://localhost:3100",
    "EnableMetrics": true,
    "PrometheusPort": 9090,
    "EnableTracing": true,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

**Características:**
- Logs no console para debugging
- Loki para agregação local
- Prometheus para métricas locais
- OTLP para traces (Tempo)

### **Produção**

```json
{
  "Observability": {
    "ServiceName": "MinhaApp",
    "EnableConsoleLogging": false,
    "LokiUrl": "",
    "EnableMetrics": false,
    "EnableTracing": true,
    "EnableLogging": true,
    "OtlpEndpoint": "http://otel-collector:4317"
  }
}
```

**Características:**
- Tudo via OTLP Collector
- Processamento centralizado
- Roteamento para múltiplos destinos
- Sem dependências diretas

### **Docker/Kubernetes**

```yaml
# docker-compose.yml
services:
  app:
    environment:
      - Observability__OtlpEndpoint=http://otel-collector:4317
      - Observability__LokiUrl=
      - Observability__EnableConsoleLogging=false
  
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    # Configuração do Collector
```

## 📊 Comparação de Estratégias

| Aspecto | Direta | OTLP | Híbrida |
|---------|--------|------|---------|
| **Complexidade** | Baixa | Média | Média |
| **Performance** | Alta | Média | Alta |
| **Flexibilidade** | Baixa | Alta | Alta |
| **Manutenção** | Média | Baixa | Média |
| **Debugging** | Fácil | Médio | Fácil |
| **Escalabilidade** | Baixa | Alta | Alta |

## 🚀 Implementação Prática

### **1. Configuração Baseada em Ambiente**

```csharp
public static class ObservabilityConfiguration
{
    public static void ConfigureObservability(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var options = new ObservabilityOptions();
        
        if (environment.IsProduction())
        {
            ConfigureForProduction(options, configuration);
        }
        else if (environment.IsDevelopment())
        {
            ConfigureForDevelopment(options, configuration);
        }
        else
        {
            ConfigureForStaging(options, configuration);
        }
        
        services.AddObservability(options);
    }
    
    private static void ConfigureForProduction(ObservabilityOptions options, IConfiguration config)
    {
        // Produção: OTLP puro
        options.ServiceName = config["ServiceName"];
        options.EnableConsoleLogging = false;
        options.LokiUrl = "";
        options.EnableMetrics = false;
        options.EnableTracing = true;
        options.EnableLogging = true;
        options.OtlpEndpoint = config["OtlpEndpoint"];
    }
    
    private static void ConfigureForDevelopment(ObservabilityOptions options, IConfiguration config)
    {
        // Desenvolvimento: Híbrida
        options.ServiceName = config["ServiceName"];
        options.EnableConsoleLogging = true;
        options.LokiUrl = "http://localhost:3100";
        options.EnableMetrics = true;
        options.EnableTracing = true;
        options.OtlpEndpoint = "http://localhost:4317";
    }
}
```

### **2. Uso no Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuração automática baseada em ambiente
builder.Services.ConfigureObservability(builder.Configuration, builder.Environment);

var app = builder.Build();

// Middleware baseado em configuração
if (builder.Environment.IsDevelopment())
{
    app.UseCustomRouteMetrics();
}

app.Run();
```

## 🔍 Monitoramento da Configuração

### **Health Checks**

```csharp
// Adicionar health checks específicos
builder.Services.AddHealthChecks()
    .AddCheck<ObservabilityHealthCheck>("observability")
    .AddCheck<LokiHealthCheck>("loki", tags: new[] { "logging" })
    .AddCheck<PrometheusHealthCheck>("prometheus", tags: new[] { "metrics" })
    .AddCheck<OtlpHealthCheck>("otlp", tags: new[] { "tracing" });
```

### **Logging da Configuração**

```csharp
public class ObservabilityStartupExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var options = configuration.GetSection("Observability").Get<ObservabilityOptions>();
        
        // Log da configuração ativa
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ObservabilityStartupExtensions>>();
        logger.LogInformation("Observability configured: " +
            "Console={Console}, Loki={Loki}, Prometheus={Prometheus}, OTLP={Otlp}",
            options.EnableConsoleLogging,
            !string.IsNullOrEmpty(options.LokiUrl),
            options.EnableMetrics,
            !string.IsNullOrEmpty(options.OtlpEndpoint));
        
        return services;
    }
}
```

## 📝 Recomendações Finais

### **Para Novos Projetos**

1. **Comece com Híbrida** - oferece flexibilidade máxima
2. **Configure por ambiente** - diferentes estratégias para diferentes contextos
3. **Documente a estratégia** - deixe claro qual abordagem está sendo usada
4. **Monitore a configuração** - health checks e logs

### **Para Projetos Existentes**

1. **Mantenha compatibilidade** - não quebre configurações existentes
2. **Adicione OTLP gradualmente** - migração incremental
3. **Teste em desenvolvimento** - valide antes de produção
4. **Documente mudanças** - changelog e guias de migração

### **Para Produção**

1. **Use OTLP puro** - centralização e padronização
2. **Configure Collector adequadamente** - roteamento e processamento
3. **Monitore o Collector** - health checks e métricas
4. **Tenha fallbacks** - configurações de backup

A arquitetura híbrida atual do Package.Observability oferece **máxima flexibilidade** e **compatibilidade**, permitindo que você escolha a melhor estratégia para cada cenário específico.
