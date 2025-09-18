using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Serilog.Events;
using Xunit;

namespace Package.Observability.UnitTests;

public class SerilogServiceTests
{
    private readonly Mock<IOptions<ObservabilityOptions>> _mockOptions;
    private readonly Mock<ILogger<SerilogService>> _mockLogger;
    private readonly ObservabilityOptions _options;

    public SerilogServiceTests()
    {
        _mockOptions = new Mock<IOptions<ObservabilityOptions>>();
        _mockLogger = new Mock<ILogger<SerilogService>>();
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
    public void Constructor_WithValidOptions_ShouldInitializeCorrectly()
    {
        // Act
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.IsConfigured.Should().BeFalse();
        service.SerilogLogger.Should().BeNull();
    }

    [Fact]
    public void Configure_WithLoggingDisabled_ShouldReturnFalse()
    {
        // Arrange
        _options.EnableLogging = false;
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);

        // Act
        var result = service.Configure();

        // Assert
        result.Should().BeFalse();
        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void Configure_WithLoggingEnabled_ShouldReturnTrue()
    {
        // Arrange
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);

        // Act
        var result = service.Configure();

        // Assert
        result.Should().BeTrue();
        service.IsConfigured.Should().BeTrue();
        service.SerilogLogger.Should().NotBeNull();
    }

    [Fact]
    public void Log_WhenConfigured_ShouldNotThrow()
    {
        // Arrange
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);
        service.Configure();

        // Act & Assert
        var action = () => service.Log(LogEventLevel.Information, "Test message");
        action.Should().NotThrow();
    }

    [Fact]
    public void Log_WhenNotConfigured_ShouldNotThrow()
    {
        // Arrange
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);

        // Act & Assert
        var action = () => service.Log(LogEventLevel.Information, "Test message");
        action.Should().NotThrow();
    }

    [Fact]
    public void GetConfigurationStatus_WhenConfigured_ShouldReturnCorrectStatus()
    {
        // Arrange
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);
        service.Configure();

        // Act
        var status = service.GetConfigurationStatus();

        // Assert
        status.Should().NotBeNull();
        status.IsConfigured.Should().BeTrue();
        status.ServiceName.Should().Be("TestService");
        status.MinimumLogLevel.Should().Be("Information");
    }

    [Fact]
    public void GetConfigurationStatus_WhenNotConfigured_ShouldReturnCorrectStatus()
    {
        // Arrange
        var service = new SerilogService(_mockOptions.Object, _mockLogger.Object);

        // Act
        var status = service.GetConfigurationStatus();

        // Assert
        status.Should().NotBeNull();
        status.IsConfigured.Should().BeFalse();
        status.ServiceName.Should().Be("TestService");
        status.SinkCount.Should().Be(0);
    }
}
