using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Package.Observability.HealthChecks;
using Xunit;

namespace Package.Observability.UnitTests;

/// <summary>
/// Testes unitários para HealthChecks com Tempo e Collector
/// </summary>
public class TempoCollectorHealthCheckTests
{
    private readonly Mock<ILogger<ObservabilityHealthCheck>> _observabilityLoggerMock;
    private readonly Mock<ILogger<TracingHealthCheck>> _tracingLoggerMock;
    private readonly Mock<IOptions<ObservabilityOptions>> _optionsMock;

    public TempoCollectorHealthCheckTests()
    {
        _observabilityLoggerMock = new Mock<ILogger<ObservabilityHealthCheck>>();
        _tracingLoggerMock = new Mock<ILogger<TracingHealthCheck>>();
        _optionsMock = new Mock<IOptions<ObservabilityOptions>>();
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithValidTempoAndCollector_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("TempoEndpoint");
        result.Data.Should().ContainKey("CollectorEndpoint");
        result.Data["TempoEndpoint"].Should().Be("http://tempo:3200");
        result.Data["CollectorEndpoint"].Should().Be("http://collector:4317");
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithInvalidTempoEndpoint_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "invalid-url",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("TempoEndpoint inválido: invalid-url");
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithInvalidCollectorEndpoint_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "invalid-url"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("CollectorEndpoint inválido: invalid-url");
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithBothInvalidEndpoints_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "invalid-tempo-url",
            CollectorEndpoint = "invalid-collector-url"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("TempoEndpoint inválido: invalid-tempo-url");
        issues.Should().Contain("CollectorEndpoint inválido: invalid-collector-url");
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithEmptyEndpoints_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "",
            CollectorEndpoint = ""
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["TempoEndpoint"].Should().Be("Não configurado");
        result.Data["CollectorEndpoint"].Should().Be("Não configurado");
    }

    [Fact]
    public async Task TracingHealthCheck_WithValidTempoAndCollector_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("TempoEndpoint");
        result.Data.Should().ContainKey("CollectorEndpoint");
        result.Data["TempoEndpoint"].Should().Be("http://tempo:3200");
        result.Data["CollectorEndpoint"].Should().Be("http://collector:4317");
    }

    [Fact]
    public async Task TracingHealthCheck_WithNoTracingEndpoints_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            OtlpEndpoint = "",
            CollectorEndpoint = ""
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("Nenhum endpoint de tracing configurado (OtlpEndpoint ou CollectorEndpoint)");
    }

    [Fact]
    public async Task TracingHealthCheck_WithOnlyOtlpEndpoint_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            OtlpEndpoint = "http://jaeger:4317",
            CollectorEndpoint = ""
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["OtlpEndpoint"].Should().Be("http://jaeger:4317");
        result.Data["CollectorEndpoint"].Should().Be("Não configurado");
    }

    [Fact]
    public async Task TracingHealthCheck_WithOnlyCollectorEndpoint_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            OtlpEndpoint = "",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["OtlpEndpoint"].Should().Be("Não configurado");
        result.Data["CollectorEndpoint"].Should().Be("http://collector:4317");
    }

    [Fact]
    public async Task TracingHealthCheck_WithInvalidTempoEndpoint_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "invalid-url",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("TempoEndpoint inválido: invalid-url");
    }

    [Fact]
    public async Task TracingHealthCheck_WithInvalidCollectorEndpoint_ShouldBeDegraded()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = true,
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "invalid-url"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("Issues");
        var issues = result.Data["Issues"] as List<string>;
        issues.Should().Contain("CollectorEndpoint inválido: invalid-url");
    }

    [Fact]
    public async Task TracingHealthCheck_WithTracingDisabled_ShouldBeHealthy()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = false,
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "http://collector:4317"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new TracingHealthCheck(_optionsMock.Object, _tracingLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Tracing desabilitado");
    }

    [Fact]
    public async Task ObservabilityHealthCheck_WithTracingDisabled_ShouldNotValidateEndpoints()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableTracing = false,
            TempoEndpoint = "invalid-url",
            CollectorEndpoint = "invalid-url"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        var healthCheck = new ObservabilityHealthCheck(_optionsMock.Object, _observabilityLoggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().NotContainKey("Issues");
    }
}
