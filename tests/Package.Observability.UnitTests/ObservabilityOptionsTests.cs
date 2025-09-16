using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ObservabilityOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.ServiceName.Should().Be("DefaultService");
        options.PrometheusPort.Should().Be(9090);
        options.EnableMetrics.Should().BeTrue();
        options.EnableTracing.Should().BeTrue();
        options.EnableLogging.Should().BeTrue();
        options.LokiUrl.Should().Be("http://localhost:3100");
        options.OtlpEndpoint.Should().Be("http://localhost:4317");
        options.EnableConsoleLogging.Should().BeTrue();
        options.MinimumLogLevel.Should().Be("Information");
        options.AdditionalLabels.Should().NotBeNull();
        options.AdditionalLabels.Should().BeEmpty();
        options.LokiLabels.Should().NotBeNull();
        options.LokiLabels.Should().BeEmpty();
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
    }

    [Fact]
    public void ServiceName_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var serviceName = "MyTestService";

        // Act
        options.ServiceName = serviceName;

        // Assert
        options.ServiceName.Should().Be(serviceName);
    }

    [Fact]
    public void PrometheusPort_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var port = 9110;

        // Act
        options.PrometheusPort = port;

        // Assert
        options.PrometheusPort.Should().Be(port);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableMetrics_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableMetrics = value;

        // Assert
        options.EnableMetrics.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableTracing_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableTracing = value;

        // Assert
        options.EnableTracing.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableLogging = value;

        // Assert
        options.EnableLogging.Should().Be(value);
    }

    [Theory]
    [InlineData("http://localhost:3100")]
    [InlineData("https://loki.example.com:3100")]
    [InlineData("http://loki.monitoring.svc.cluster.local:3100")]
    [InlineData("")]
    [InlineData(null)]
    public void LokiUrl_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.LokiUrl = value;

        // Assert
        options.LokiUrl.Should().Be(value);
    }

    [Theory]
    [InlineData("http://localhost:4317")]
    [InlineData("https://jaeger.example.com:4317")]
    [InlineData("http://jaeger.monitoring.svc.cluster.local:4317")]
    [InlineData("")]
    [InlineData(null)]
    public void OtlpEndpoint_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.OtlpEndpoint = value;

        // Assert
        options.OtlpEndpoint.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableConsoleLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableConsoleLogging = value;

        // Assert
        options.EnableConsoleLogging.Should().Be(value);
    }

    [Theory]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Fatal")]
    public void MinimumLogLevel_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.MinimumLogLevel = value;

        // Assert
        options.MinimumLogLevel.Should().Be(value);
    }

    [Fact]
    public void AdditionalLabels_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var labels = new Dictionary<string, string>
        {
            ["environment"] = "production",
            ["version"] = "1.0.0",
            ["team"] = "backend"
        };

        // Act
        options.AdditionalLabels = labels;

        // Assert
        options.AdditionalLabels.Should().BeEquivalentTo(labels);
    }

    [Fact]
    public void AdditionalLabels_CanBeModified()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.AdditionalLabels.Add("environment", "production");
        options.AdditionalLabels.Add("version", "1.0.0");

        // Assert
        options.AdditionalLabels.Should().HaveCount(2);
        options.AdditionalLabels["environment"].Should().Be("production");
        options.AdditionalLabels["version"].Should().Be("1.0.0");
    }

    [Fact]
    public void LokiLabels_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var labels = new Dictionary<string, string>
        {
            ["app"] = "my-app",
            ["component"] = "api",
            ["tier"] = "backend"
        };

        // Act
        options.LokiLabels = labels;

        // Assert
        options.LokiLabels.Should().BeEquivalentTo(labels);
    }

    [Fact]
    public void LokiLabels_CanBeModified()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.LokiLabels.Add("app", "my-app");
        options.LokiLabels.Add("component", "api");

        // Assert
        options.LokiLabels.Should().HaveCount(2);
        options.LokiLabels["app"].Should().Be("my-app");
        options.LokiLabels["component"].Should().Be("api");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableCorrelationId_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableCorrelationId = value;

        // Assert
        options.EnableCorrelationId.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableRuntimeInstrumentation_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableRuntimeInstrumentation = value;

        // Assert
        options.EnableRuntimeInstrumentation.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableHttpClientInstrumentation_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableHttpClientInstrumentation = value;

        // Assert
        options.EnableHttpClientInstrumentation.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableAspNetCoreInstrumentation_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableAspNetCoreInstrumentation = value;

        // Assert
        options.EnableAspNetCoreInstrumentation.Should().Be(value);
    }

    [Fact]
    public void AllProperties_CanBeSetSimultaneously()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var additionalLabels = new Dictionary<string, string> { ["env"] = "test" };
        var lokiLabels = new Dictionary<string, string> { ["app"] = "test-app" };

        // Act
        options.ServiceName = "TestService";
        options.PrometheusPort = 9110;
        options.EnableMetrics = false;
        options.EnableTracing = false;
        options.EnableLogging = false;
        options.LokiUrl = "http://test-loki:3100";
        options.OtlpEndpoint = "http://test-jaeger:4317";
        options.EnableConsoleLogging = false;
        options.MinimumLogLevel = "Debug";
        options.AdditionalLabels = additionalLabels;
        options.LokiLabels = lokiLabels;
        options.EnableCorrelationId = false;
        options.EnableRuntimeInstrumentation = false;
        options.EnableHttpClientInstrumentation = false;
        options.EnableAspNetCoreInstrumentation = false;

        // Assert
        options.ServiceName.Should().Be("TestService");
        options.PrometheusPort.Should().Be(9110);
        options.EnableMetrics.Should().BeFalse();
        options.EnableTracing.Should().BeFalse();
        options.EnableLogging.Should().BeFalse();
        options.LokiUrl.Should().Be("http://test-loki:3100");
        options.OtlpEndpoint.Should().Be("http://test-jaeger:4317");
        options.EnableConsoleLogging.Should().BeFalse();
        options.MinimumLogLevel.Should().Be("Debug");
        options.AdditionalLabels.Should().BeEquivalentTo(additionalLabels);
        options.LokiLabels.Should().BeEquivalentTo(lokiLabels);
        options.EnableCorrelationId.Should().BeFalse();
        options.EnableRuntimeInstrumentation.Should().BeFalse();
        options.EnableHttpClientInstrumentation.Should().BeFalse();
        options.EnableAspNetCoreInstrumentation.Should().BeFalse();
    }

    [Fact]
    public void AdditionalLabels_IsNotNullAfterConstruction()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.AdditionalLabels.Should().NotBeNull();
    }

    [Fact]
    public void LokiLabels_IsNotNullAfterConstruction()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.LokiLabels.Should().NotBeNull();
    }

    [Fact]
    public void AdditionalLabels_CanBeSetToNull()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.AdditionalLabels = null!;

        // Assert
        options.AdditionalLabels.Should().BeNull();
    }

    [Fact]
    public void LokiLabels_CanBeSetToNull()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.LokiLabels = null!;

        // Assert
        options.LokiLabels.Should().BeNull();
    }
}
