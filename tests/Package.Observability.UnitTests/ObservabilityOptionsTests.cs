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
        options.TempoEndpoint.Should().Be("http://localhost:3200");
        options.CollectorEndpoint.Should().Be("http://localhost:4317");
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
        
        // Serilog-specific defaults
        options.EnableFileLogging.Should().BeFalse();
        options.FileLoggingPath.Should().BeNull();
        options.EnableSeqLogging.Should().BeFalse();
        options.SeqUrl.Should().BeNull();
        options.EnableElasticsearchLogging.Should().BeFalse();
        options.ElasticsearchUrl.Should().BeNull();
        options.ConsoleOutputTemplate.Should().BeNull();
        options.FileOutputTemplate.Should().BeNull();
        options.EnableRequestLogging.Should().BeTrue();
        options.SlowRequestThreshold.Should().Be(1000);
        options.AdditionalEnrichers.Should().NotBeNull();
        options.AdditionalEnrichers.Should().BeEmpty();
        options.CustomProperties.Should().NotBeNull();
        options.CustomProperties.Should().BeEmpty();
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
    [InlineData("http://localhost:3200")]
    [InlineData("https://tempo.example.com:3200")]
    [InlineData("http://tempo.monitoring.svc.cluster.local:3200")]
    [InlineData("")]
    [InlineData(null)]
    public void TempoEndpoint_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.TempoEndpoint = value;

        // Assert
        options.TempoEndpoint.Should().Be(value);
    }

    [Theory]
    [InlineData("http://localhost:4317")]
    [InlineData("https://collector.example.com:4317")]
    [InlineData("http://collector.monitoring.svc.cluster.local:4317")]
    [InlineData("")]
    [InlineData(null)]
    public void CollectorEndpoint_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.CollectorEndpoint = value;

        // Assert
        options.CollectorEndpoint.Should().Be(value);
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

    // Serilog-specific property tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableFileLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableFileLogging = value;

        // Assert
        options.EnableFileLogging.Should().Be(value);
    }

    [Theory]
    [InlineData("test.log")]
    [InlineData("logs/app-.log")]
    [InlineData("")]
    [InlineData(null)]
    public void FileLoggingPath_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.FileLoggingPath = value;

        // Assert
        options.FileLoggingPath.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSeqLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableSeqLogging = value;

        // Assert
        options.EnableSeqLogging.Should().Be(value);
    }

    [Theory]
    [InlineData("http://localhost:5341")]
    [InlineData("https://seq.example.com:5341")]
    [InlineData("")]
    [InlineData(null)]
    public void SeqUrl_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.SeqUrl = value;

        // Assert
        options.SeqUrl.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableElasticsearchLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableElasticsearchLogging = value;

        // Assert
        options.EnableElasticsearchLogging.Should().Be(value);
    }

    [Theory]
    [InlineData("http://localhost:9200")]
    [InlineData("https://elasticsearch.example.com:9200")]
    [InlineData("")]
    [InlineData(null)]
    public void ElasticsearchUrl_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.ElasticsearchUrl = value;

        // Assert
        options.ElasticsearchUrl.Should().Be(value);
    }

    [Theory]
    [InlineData("[{Timestamp}] {Message}")]
    [InlineData("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}")]
    [InlineData("")]
    [InlineData(null)]
    public void ConsoleOutputTemplate_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.ConsoleOutputTemplate = value;

        // Assert
        options.ConsoleOutputTemplate.Should().Be(value);
    }

    [Theory]
    [InlineData("{Timestamp:yyyy-MM-dd HH:mm:ss} {Message}")]
    [InlineData("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}")]
    [InlineData("")]
    [InlineData(null)]
    public void FileOutputTemplate_CanBeSet(string value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.FileOutputTemplate = value;

        // Assert
        options.FileOutputTemplate.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableRequestLogging_CanBeSet(bool value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.EnableRequestLogging = value;

        // Assert
        options.EnableRequestLogging.Should().Be(value);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(null)]
    public void SlowRequestThreshold_CanBeSet(int? value)
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.SlowRequestThreshold = value;

        // Assert
        options.SlowRequestThreshold.Should().Be(value);
    }

    [Fact]
    public void AdditionalEnrichers_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var enrichers = new List<string> { "TestEnricher", "AnotherEnricher" };

        // Act
        options.AdditionalEnrichers = enrichers;

        // Assert
        options.AdditionalEnrichers.Should().BeEquivalentTo(enrichers);
    }

    [Fact]
    public void AdditionalEnrichers_CanBeModified()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.AdditionalEnrichers.Add("TestEnricher");
        options.AdditionalEnrichers.Add("AnotherEnricher");

        // Assert
        options.AdditionalEnrichers.Should().HaveCount(2);
        options.AdditionalEnrichers.Should().Contain("TestEnricher");
        options.AdditionalEnrichers.Should().Contain("AnotherEnricher");
    }

    [Fact]
    public void CustomProperties_CanBeSet()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var properties = new Dictionary<string, object>
        {
            ["Version"] = "1.0.0",
            ["Environment"] = "Test",
            ["Count"] = 42
        };

        // Act
        options.CustomProperties = properties;

        // Assert
        options.CustomProperties.Should().BeEquivalentTo(properties);
    }

    [Fact]
    public void CustomProperties_CanBeModified()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.CustomProperties.Add("Version", "1.0.0");
        options.CustomProperties.Add("Environment", "Test");
        options.CustomProperties.Add("Count", 42);

        // Assert
        options.CustomProperties.Should().HaveCount(3);
        options.CustomProperties["Version"].Should().Be("1.0.0");
        options.CustomProperties["Environment"].Should().Be("Test");
        options.CustomProperties["Count"].Should().Be(42);
    }

    [Fact]
    public void AdditionalEnrichers_IsNotNullAfterConstruction()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.AdditionalEnrichers.Should().NotBeNull();
    }

    [Fact]
    public void CustomProperties_IsNotNullAfterConstruction()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.CustomProperties.Should().NotBeNull();
    }

    [Fact]
    public void AdditionalEnrichers_CanBeSetToNull()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.AdditionalEnrichers = null!;

        // Assert
        options.AdditionalEnrichers.Should().BeNull();
    }

    [Fact]
    public void CustomProperties_CanBeSetToNull()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.CustomProperties = null!;

        // Assert
        options.CustomProperties.Should().BeNull();
    }

    [Fact]
    public void AllSerilogProperties_CanBeSetSimultaneously()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var additionalEnrichers = new List<string> { "TestEnricher" };
        var customProperties = new Dictionary<string, object> { ["Version"] = "1.0.0" };

        // Act
        options.EnableFileLogging = true;
        options.FileLoggingPath = "test.log";
        options.EnableSeqLogging = true;
        options.SeqUrl = "http://localhost:5341";
        options.EnableElasticsearchLogging = true;
        options.ElasticsearchUrl = "http://localhost:9200";
        options.ConsoleOutputTemplate = "[{Timestamp}] {Message}";
        options.FileOutputTemplate = "{Timestamp:yyyy-MM-dd} {Message}";
        options.EnableRequestLogging = false;
        options.SlowRequestThreshold = 2000;
        options.AdditionalEnrichers = additionalEnrichers;
        options.CustomProperties = customProperties;

        // Assert
        options.EnableFileLogging.Should().BeTrue();
        options.FileLoggingPath.Should().Be("test.log");
        options.EnableSeqLogging.Should().BeTrue();
        options.SeqUrl.Should().Be("http://localhost:5341");
        options.EnableElasticsearchLogging.Should().BeTrue();
        options.ElasticsearchUrl.Should().Be("http://localhost:9200");
        options.ConsoleOutputTemplate.Should().Be("[{Timestamp}] {Message}");
        options.FileOutputTemplate.Should().Be("{Timestamp:yyyy-MM-dd} {Message}");
        options.EnableRequestLogging.Should().BeFalse();
        options.SlowRequestThreshold.Should().Be(2000);
        options.AdditionalEnrichers.Should().BeEquivalentTo(additionalEnrichers);
        options.CustomProperties.Should().BeEquivalentTo(customProperties);
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
