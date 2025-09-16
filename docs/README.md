# 📚 Documentação - Package.Observability

Bem-vindo à documentação completa do `Package.Observability`! Esta documentação foi criada para ajudá-lo a usar o pacote de forma eficiente e flexível.

## 🚀 Começando

### [Quick Start](quick-start.md)
**Comece em 30 segundos!** Guia rápido para configurar o pacote e começar a usar imediatamente.

### [Guia de Uso Completo](usage-guide.md)
Documentação detalhada e abrangente sobre como usar o pacote em diferentes cenários.

### [Exemplos de Configuração](configuration-examples.md)
Exemplos práticos de configuração para diferentes ambientes e necessidades.

### [FAQ](faq.md)
Perguntas frequentes e respostas sobre o uso do pacote.

## 🎯 Cenários de Uso

### Desenvolvimento Local
- Apenas logs no console
- Métricas locais (sem Prometheus)
- Sem tracing distribuído

### Staging
- Logs estruturados (Loki)
- Métricas (Prometheus)
- Tracing (Jaeger)

### Produção
- Observabilidade completa
- Health checks
- Monitoramento e alertas

## 🔧 Configurações Flexíveis

O `Package.Observability` oferece máxima flexibilidade:

### ✅ Apenas Logs
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = false;
    options.EnableLogging = true;
    options.EnableConsoleLogging = true;
    options.LokiUrl = ""; // Sem Loki
});
```

### ✅ Apenas Métricas
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = false;
    options.EnableLogging = false;
    options.PrometheusPort = 9090;
});
```

### ✅ Apenas Tracing
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = false;
    options.EnableTracing = true;
    options.EnableLogging = false;
    options.OtlpEndpoint = "http://jaeger:4317";
});
```

### ✅ Configuração Completa
```csharp
builder.Services.AddObservability(options =>
{
    options.ServiceName = "MeuServico";
    options.EnableMetrics = true;
    options.EnableTracing = true;
    options.EnableLogging = true;
    options.LokiUrl = "http://loki:3100";
    options.OtlpEndpoint = "http://jaeger:4317";
});
```

## 📊 Componentes Disponíveis

### Métricas (Prometheus)
- **Runtime .NET**: GC, threads, exceções
- **ASP.NET Core**: Requisições HTTP, duração, status codes
- **HTTP Client**: Requisições outbound, duração, status codes
- **Métricas customizadas**: Definidas pela aplicação

### Logs (Serilog)
- **Console**: Formatação legível para desenvolvimento
- **Loki**: Agregação centralizada para produção
- **Correlation ID**: Rastreamento de requisições
- **Enrichers**: Informações de processo, thread, ambiente

### Tracing (OpenTelemetry)
- **OpenTelemetry**: Padrão da indústria
- **OTLP Export**: Compatível com Jaeger, Zipkin, etc.
- **Instrumentação automática**: ASP.NET Core, HTTP Client
- **Traces customizados**: Via ActivitySource

## 🛠️ Funcionalidades Avançadas

### Validação de Configuração
- Validação automática de URLs e portas
- Mensagens de erro claras
- Fallback para configuração mínima

### Gerenciamento de Recursos
- Dispose automático de recursos
- Thread-safe
- Prevenção de vazamentos de memória

### Health Checks
- Verificação de componentes de observabilidade
- Health checks customizados
- Endpoints de saúde

### Tratamento de Erros
- Exceções customizadas
- Logging de erros
- Recuperação automática

## 🐳 Docker e Kubernetes

### Docker Compose
- Configurações para desenvolvimento
- Configurações para produção
- Stack completa de observabilidade

### Kubernetes
- ConfigMaps para configuração
- Secrets para credenciais
- Deployments com observabilidade

## 📈 Monitoramento e Alertas

### Métricas Importantes
- `http_requests_total` - Total de requisições HTTP
- `http_request_duration_seconds` - Duração das requisições
- `dotnet_gc_collections_total` - Coleta de lixo
- `dotnet_threadpool_threads_total` - Threads do pool
- `dotnet_exceptions_total` - Total de exceções

### Logs Importantes
- `ERROR` - Erros críticos
- `WARN` - Avisos importantes
- `FATAL` - Erros fatais

### Traces Importantes
- Duração de operações críticas
- Erros em cadeias de chamadas
- Performance de queries de banco

## 🔍 Troubleshooting

### Problemas Comuns
- Métricas não aparecem no Prometheus
- Logs não aparecem no Loki
- Traces não aparecem no Jaeger

### Debug de Configuração
- Log de configuração
- Validação de endpoints
- Verificação de conectividade

## 🎯 Melhores Práticas

### Configuração
- Use configuração baseada em ambiente
- Sempre tenha um fallback
- Valide configurações em startup
- Use labels consistentes

### Métricas
- Crie métricas com nomes descritivos
- Use labels para dimensões importantes
- Evite alta cardinalidade
- Documente suas métricas

### Logs
- Use logs estruturados
- Inclua contexto relevante
- Use níveis apropriados
- Evite logs excessivos

### Tracing
- Crie activities para operações importantes
- Use tags para contexto
- Mantenha traces com duração razoável
- Use correlation IDs

## 📚 Exemplos por Tipo de Aplicação

### ASP.NET Core Web API
- Configuração completa
- Controllers com métricas
- Logging estruturado
- Tracing de requisições

### Worker Service
- Background services
- Logging de workers
- Métricas de processamento
- Health checks

### Console Application
- Aplicações de linha de comando
- Logging de console
- Métricas de execução
- Tracing de operações

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor:

1. Abra uma issue para discussão
2. Faça um fork do repositório
3. Crie uma branch para sua feature
4. Faça commit das mudanças
5. Abra um pull request

## 📞 Suporte

Para dúvidas ou problemas:

1. Consulte a documentação
2. Verifique as issues existentes
3. Abra uma nova issue
4. Entre em contato com a equipe

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](../LICENSE).

---

**Pronto para começar?** Consulte o [Quick Start](quick-start.md) para começar em 30 segundos! 🚀