using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Package.Observability.HealthChecks;
using Xunit;

namespace Package.Observability.UnitTests;

public class SerilogHealthCheckTests
{
    private readonly Mock<ISerilogService> _mockSerilogService;
    private readonly Mock<IOptions<ObservabilityOptions>> _mockOptions;
    private readonly Mock<ILogger<SerilogHealthCheck>> _mockLogger;
    private readonly ObservabilityOptions _options;

    public SerilogHealthCheckTests()
    {
        _mockSerilogService = new Mock<ISerilogService>();
        _mockOptions = new Mock<IOptions<ObservabilityOptions>>();
        _mockLogger = new Mock<ILogger<SerilogHealthCheck>>();
        _options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableLogging = true,
            EnableConsoleLogging = true,
            MinimumLogLevel = "Information"
        };
        _mockOptions.Setup(x => x.Value).Returns(_options);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenLoggingDisabled_ShouldReturnHealthy()
    {
        // Arrange
        _options.EnableLogging = false;
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Logging desabilitado");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenSerilogNotConfigured_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = false,
            IsLoggingEnabled = true,
            SinkCount = 0
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoSinksEnabled_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 0
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenLokiEnabledButUrlInvalid_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.LokiUrl = "invalid-url";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsLokiEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenSeqEnabledButUrlInvalid_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.EnableSeqLogging = true;
        _options.SeqUrl = "invalid-url";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenElasticsearchEnabledButUrlInvalid_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.EnableElasticsearchLogging = true;
        _options.ElasticsearchUrl = "invalid-url";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenInvalidLogLevel_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.MinimumLogLevel = "InvalidLevel";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenInvalidSlowRequestThreshold_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.SlowRequestThreshold = -1;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenFileLoggingEnabledButDirectoryCannotBeCreated_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.EnableFileLogging = true;
        _options.FileLoggingPath = "/invalid/path/that/cannot/be/created/test.log";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsFileLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenLoggingTestFails_ShouldReturnDegraded()
    {
        // Arrange
        _options.EnableLogging = true;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        _mockSerilogService.Setup(x => x.Log(It.IsAny<Serilog.Events.LogEventLevel>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Throws(new Exception("Logging test failed"));
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("SerilogService com problemas de configuração");
        result.Data.Should().ContainKey("Issues");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEverythingIsValid_ShouldReturnHealthy()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.EnableConsoleLogging = true;
        _options.LokiUrl = "http://localhost:3100";
        _options.MinimumLogLevel = "Information";
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsConsoleLoggingEnabled = true,
            IsLokiEnabled = true,
            SinkCount = 2,
            ServiceName = "TestService",
            MinimumLogLevel = "Information"
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("SerilogService funcionando corretamente");
        result.Data.Should().ContainKey("IsConfigured");
        result.Data.Should().ContainKey("SinkCount");
        result.Data.Should().ContainKey("IsLokiEnabled");
        result.Data.Should().ContainKey("ServiceName");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenExceptionOccurs_ShouldReturnUnhealthy()
    {
        // Arrange
        _options.EnableLogging = true;
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Throws(new Exception("Test exception"));
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Erro ao verificar SerilogService");
        result.Exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData("http://localhost:3100")]
    [InlineData("https://loki.example.com:3100")]
    [InlineData("http://loki.monitoring.svc.cluster.local:3100")]
    public async Task CheckHealthAsync_WithValidUrls_ShouldReturnHealthy(string url)
    {
        // Arrange
        _options.EnableLogging = true;
        _options.LokiUrl = url;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsLokiEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://localhost:3100")]
    [InlineData("not-a-url")]
    public async Task CheckHealthAsync_WithInvalidUrls_ShouldReturnDegraded(string url)
    {
        // Arrange
        _options.EnableLogging = true;
        _options.LokiUrl = url;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsLokiEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Theory]
    [InlineData("Trace")]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Critical")]
    [InlineData("Fatal")]
    public async Task CheckHealthAsync_WithValidLogLevels_ShouldReturnHealthy(string logLevel)
    {
        // Arrange
        _options.EnableLogging = true;
        _options.MinimumLogLevel = logLevel;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            SinkCount = 1
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeAllRelevantData()
    {
        // Arrange
        _options.EnableLogging = true;
        _options.EnableConsoleLogging = true;
        _options.EnableFileLogging = true;
        _options.LokiUrl = "http://localhost:3100";
        _options.SeqUrl = "http://localhost:5341";
        _options.FileLoggingPath = "test.log";
        _options.EnableRequestLogging = true;
        _options.SlowRequestThreshold = 1000;
        var status = new SerilogConfigurationStatus
        {
            IsConfigured = true,
            IsLoggingEnabled = true,
            IsConsoleLoggingEnabled = true,
            IsFileLoggingEnabled = true,
            IsLokiEnabled = true,
            SinkCount = 3,
            ServiceName = "TestService",
            MinimumLogLevel = "Information"
        };
        _mockSerilogService.Setup(x => x.GetConfigurationStatus()).Returns(status);
        var healthCheck = new SerilogHealthCheck(_mockSerilogService.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Data.Should().ContainKey("IsConfigured");
        result.Data.Should().ContainKey("IsLoggingEnabled");
        result.Data.Should().ContainKey("IsConsoleLoggingEnabled");
        result.Data.Should().ContainKey("IsFileLoggingEnabled");
        result.Data.Should().ContainKey("IsLokiEnabled");
        result.Data.Should().ContainKey("MinimumLogLevel");
        result.Data.Should().ContainKey("ServiceName");
        result.Data.Should().ContainKey("SinkCount");
        result.Data.Should().ContainKey("LokiUrl");
        result.Data.Should().ContainKey("SeqUrl");
        result.Data.Should().ContainKey("FileLoggingPath");
        result.Data.Should().ContainKey("EnableRequestLogging");
        result.Data.Should().ContainKey("SlowRequestThreshold");
    }
}
