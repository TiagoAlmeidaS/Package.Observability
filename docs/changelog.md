# Changelog

Histórico de mudanças do Package.Observability.

## [1.0.0] - 2024-01-15

### Added
- Configuração completa de observabilidade para .NET 8
- Suporte a métricas com Prometheus
- Suporte a logs estruturados com Serilog e Loki
- Suporte a traces distribuídos com OpenTelemetry
- Configuração flexível via appsettings.json ou código
- Correlation ID automático
- Instrumentação automática de runtime, ASP.NET Core e HTTP Client
- Factory para criação de métricas customizadas
- Factory para criação de traces customizados
- Documentação completa com exemplos
- Docker Compose para stack de observabilidade
- Testes de integração básicos

### Features
- Métricas automáticas de runtime .NET
- Métricas automáticas de ASP.NET Core
- Métricas automáticas de HTTP Client
- Logs estruturados com enrichers
- Traces distribuídos com OTLP
- Configuração por ambiente
- Labels customizados para métricas e logs
- Endpoint de métricas Prometheus
- Suporte a múltiplos tipos de aplicação (Web API, Worker, Console)

## [Unreleased]

### Planned
- Testes unitários completos
- Suporte a health checks
- Métricas de performance customizadas
- Suporte a múltiplos backends de logs
- Suporte a múltiplos backends de traces
- Configuração via atributos
- Suporte a .NET 9
- Métricas de negócio automáticas
- Dashboard do Grafana incluído
- Suporte a Kubernetes
