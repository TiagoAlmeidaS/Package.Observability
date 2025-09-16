# Troubleshooting

Guia para solução de problemas comuns com o Package.Observability.

## 🚨 Problemas Comuns

### 1. Aplicação não inicia

#### Sintomas
- Aplicação falha ao iniciar
- Erro de configuração
- Dependências não encontradas

#### Possíveis Causas
- Dependências não instaladas
- Configuração inválida
- URLs inacessíveis

#### Soluções

**Verificar dependências:**
```bash
dotnet restore
dotnet build
```

**Verificar configuração:**
```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao",
    "EnableMetrics": true,
    "EnableTracing": true,
    "EnableLogging": true
  }
}
```

**Verificar URLs:**
```csharp
// Verificar se as URLs estão acessíveis
var lokiUrl = "http://localhost:3100";
var otlpEndpoint = "http://localhost:4317";
```

### 2. Métricas não aparecem

#### Sintomas
- Endpoint `/metrics` retorna vazio
- Prometheus não coleta métricas
- Métricas customizadas não aparecem

#### Possíveis Causas
- `EnableMetrics` está `false`
- Endpoint não mapeado
- Nenhuma requisição sendo feita
- Meter não registrado

#### Soluções

**Verificar configuração:**
```csharp
builder.Services.AddObservability(options =>
{
    options.EnableMetrics = true; // Deve ser true
    options.EnableRuntimeInstrumentation = true;
    options.EnableAspNetCoreInstrumentation = true;
});
```

**Mapear endpoint:**
```csharp
var app = builder.Build();
app.MapPrometheusScrapingEndpoint(); // Deve estar presente
```

**Fazer requisições:**
```bash
# Fazer requisições para gerar métricas
curl http://localhost:5000/api/endpoint
```

**Verificar métricas:**
```bash
# Acessar endpoint de métricas
curl http://localhost:9090/metrics
```

### 3. Logs não aparecem no Loki

#### Sintomas
- Logs aparecem no console mas não no Loki
- Loki não recebe logs
- Erro de conexão com Loki

#### Possíveis Causas
- `LokiUrl` incorreto
- Loki não está rodando
- `EnableLogging` está `false`
- Problema de rede

#### Soluções

**Verificar configuração:**
```csharp
builder.Services.AddObservability(options =>
{
    options.EnableLogging = true; // Deve ser true
    options.LokiUrl = "http://localhost:3100"; // URL correta
});
```

**Verificar se Loki está rodando:**
```bash
# Verificar se Loki está acessível
curl http://localhost:3100/ready
```

**Verificar logs de erro:**
```csharp
// Habilitar logs de debug para ver erros
builder.Services.AddObservability(options =>
{
    options.MinimumLogLevel = "Debug";
});
```

### 4. Traces não aparecem no Jaeger

#### Sintomas
- Traces não aparecem no Jaeger
- Jaeger não recebe traces
- Erro de conexão com OTLP

#### Possíveis Causas
- `OtlpEndpoint` incorreto
- Jaeger não está rodando
- `EnableTracing` está `false`
- Problema de rede

#### Soluções

**Verificar configuração:**
```csharp
builder.Services.AddObservability(options =>
{
    options.EnableTracing = true; // Deve ser true
    options.OtlpEndpoint = "http://localhost:4317"; // URL correta
});
```

**Verificar se Jaeger está rodando:**
```bash
# Verificar se Jaeger está acessível
curl http://localhost:16686/api/services
```

**Verificar traces:**
```csharp
// Criar trace customizado para testar
using var activity = ActivitySourceFactory.StartActivity("TestService", "TestActivity");
activity?.SetTag("test", "value");
```

### 5. Erro de configuração

#### Sintomas
- `ConfigurationSectionNotFoundException`
- `InvalidOperationException`
- Configuração não encontrada

#### Possíveis Causas
- Seção "Observability" não existe
- Configuração malformada
- Tipos incorretos

#### Soluções

**Verificar seção de configuração:**
```json
{
  "Observability": {
    "ServiceName": "MinhaAplicacao"
  }
}
```

**Validar configuração:**
```csharp
var options = builder.Configuration.GetSection("Observability").Get<ObservabilityOptions>();
if (options == null)
{
    throw new InvalidOperationException("Configuração de Observability não encontrada");
}
```

**Usar configuração por código:**
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    // Outras configurações...
});
```

### 6. Performance degradada

#### Sintomas
- Aplicação lenta
- Alto uso de CPU
- Alto uso de memória

#### Possíveis Causas
- Instrumentação excessiva
- Métricas muito frequentes
- Logs muito verbosos
- Traces muito detalhados

#### Soluções

**Reduzir instrumentação:**
```csharp
builder.Services.AddObservability(options =>
{
    options.EnableRuntimeInstrumentation = false; // Desabilitar se não necessário
    options.EnableHttpClientInstrumentation = false; // Desabilitar se não necessário
});
```

**Ajustar nível de log:**
```csharp
builder.Services.AddObservability(options =>
{
    options.MinimumLogLevel = "Warning"; // Menos verboso
});
```

**Otimizar métricas:**
```csharp
// Usar métricas apenas quando necessário
if (shouldRecordMetrics)
{
    _counter.Add(1);
}
```

### 7. Erro de dependências

#### Sintomas
- `FileNotFoundException`
- `TypeLoadException`
- Dependências não encontradas

#### Possíveis Causas
- Versões incompatíveis
- Dependências faltando
- Conflitos de versão

#### Soluções

**Verificar versões:**
```xml
<PackageReference Include="Package.Observability" Version="1.0.0" />
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
```

**Limpar e restaurar:**
```bash
dotnet clean
dotnet restore
dotnet build
```

**Verificar conflitos:**
```bash
dotnet list package --include-transitive
```

## 🔍 Diagnóstico

### 1. Verificar Configuração

```csharp
// Logar configuração para debug
var options = builder.Configuration.GetSection("Observability").Get<ObservabilityOptions>();
Console.WriteLine($"ServiceName: {options?.ServiceName}");
Console.WriteLine($"EnableMetrics: {options?.EnableMetrics}");
Console.WriteLine($"EnableTracing: {options?.EnableTracing}");
Console.WriteLine($"EnableLogging: {options?.EnableLogging}");
```

### 2. Verificar Endpoints

```bash
# Verificar endpoint de métricas
curl http://localhost:9090/metrics

# Verificar endpoint de health
curl http://localhost:5000/health

# Verificar endpoint da aplicação
curl http://localhost:5000/api/endpoint
```

### 3. Verificar Logs

```csharp
// Habilitar logs de debug
builder.Services.AddObservability(options =>
{
    options.MinimumLogLevel = "Debug";
    options.EnableConsoleLogging = true;
});
```

### 4. Verificar Métricas

```csharp
// Verificar se métricas estão sendo criadas
var meter = ObservabilityMetrics.GetOrCreateMeter("TestService");
var counter = meter.CreateCounter<int>("test_counter");
counter.Add(1);
```

### 5. Verificar Traces

```csharp
// Verificar se traces estão sendo criados
using var activity = ActivitySourceFactory.StartActivity("TestService", "TestActivity");
activity?.SetTag("test", "value");
```

## 🛠️ Ferramentas de Debug

### 1. Prometheus

```bash
# Verificar métricas no Prometheus
curl http://localhost:9090/api/v1/query?query=up
```

### 2. Loki

```bash
# Verificar logs no Loki
curl http://localhost:3100/loki/api/v1/query?query={job="MinhaAplicacao"}
```

### 3. Jaeger

```bash
# Verificar traces no Jaeger
curl http://localhost:16686/api/services
```

### 4. .NET Diagnostics

```csharp
// Habilitar diagnósticos do .NET
builder.Services.AddObservability(options =>
{
    options.EnableRuntimeInstrumentation = true;
});
```

## 📊 Monitoramento de Saúde

### 1. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("observability", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
```

### 2. Métricas de Saúde

```csharp
var healthCounter = ObservabilityMetrics.CreateCounter<int>("MinhaAplicacao", "health_checks_total");
var healthHistogram = ObservabilityMetrics.CreateHistogram<double>("MinhaAplicacao", "health_check_duration");
```

### 3. Logs de Saúde

```csharp
_logger.LogInformation("Aplicação saudável. Uptime: {Uptime}", uptime);
```

## 🚀 Otimizações

### 1. Configuração Otimizada para Produção

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao";
    options.EnableConsoleLogging = false; // Desabilitar em produção
    options.MinimumLogLevel = "Information"; // Menos verboso
    options.EnableRuntimeInstrumentation = true; // Manter métricas de runtime
    options.EnableAspNetCoreInstrumentation = true; // Manter métricas de ASP.NET Core
    options.EnableHttpClientInstrumentation = false; // Desabilitar se não necessário
});
```

### 2. Configuração Otimizada para Desenvolvimento

```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MinhaAplicacao-Dev";
    options.EnableConsoleLogging = true; // Habilitar para debug
    options.MinimumLogLevel = "Debug"; // Mais verboso
    options.EnableRuntimeInstrumentation = true;
    options.EnableAspNetCoreInstrumentation = true;
    options.EnableHttpClientInstrumentation = true;
});
```

## 📚 Recursos Adicionais

- [Guia de Início Rápido](getting-started.md)
- [Configuração](configuration.md)
- [Exemplos](examples.md)
- [API Reference](api-reference.md)
