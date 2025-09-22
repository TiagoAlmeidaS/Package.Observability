using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

/// <summary>
/// Testes unitários específicos para Tempo e OpenTelemetry Collector
/// </summary>
public class TempoCollectorUnitTests
{
    [Theory]
    [InlineData("http://localhost:3200")]
    [InlineData("https://tempo.example.com:3200")]
    [InlineData("http://tempo.monitoring.svc.cluster.local:3200")]
    [InlineData("http://192.168.1.100:3200")]
    [InlineData("https://tempo.company.com:3200")]
    public void TempoEndpoint_ValidUrls_ShouldBeAccepted(string url)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = url
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain($"TempoEndpoint inválido: {url}");
    }

    [Theory]
    [InlineData("http://localhost:4317")]
    [InlineData("https://collector.example.com:4317")]
    [InlineData("http://otel-collector.monitoring.svc.cluster.local:4317")]
    [InlineData("http://192.168.1.100:4317")]
    [InlineData("https://collector.company.com:4317")]
    public void CollectorEndpoint_ValidUrls_ShouldBeAccepted(string url)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            CollectorEndpoint = url
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain($"CollectorEndpoint inválido: {url}");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://tempo:3200")]
    [InlineData("tempo:3200")]
    [InlineData("http://")]
    [InlineData("https://")]
    public void TempoEndpoint_InvalidUrls_ShouldBeRejected(string url)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = url
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain($"TempoEndpoint inválido: {url}");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://collector:4317")]
    [InlineData("collector:4317")]
    [InlineData("http://")]
    [InlineData("https://")]
    public void CollectorEndpoint_InvalidUrls_ShouldBeRejected(string url)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            CollectorEndpoint = url
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain($"CollectorEndpoint inválido: {url}");
    }

    [Fact]
    public void TempoEndpoint_EmptyString_ShouldBeValid()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = ""
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Contains("TempoEndpoint"));
    }

    [Fact]
    public void CollectorEndpoint_EmptyString_ShouldBeValid()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            CollectorEndpoint = ""
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Contains("CollectorEndpoint"));
    }

    [Fact]
    public void TempoEndpoint_Null_ShouldBeValid()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = null!
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Contains("TempoEndpoint"));
    }

    [Fact]
    public void CollectorEndpoint_Null_ShouldBeValid()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            CollectorEndpoint = null!
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Contains("CollectorEndpoint"));
    }

    [Fact]
    public void BothEndpoints_ValidUrls_ShouldBeValid()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = "http://tempo:3200",
            CollectorEndpoint = "http://collector:4317"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void BothEndpoints_InvalidUrls_ShouldHaveMultipleErrors()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = "invalid-tempo-url",
            CollectorEndpoint = "invalid-collector-url"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("TempoEndpoint inválido: invalid-tempo-url");
        result.Errors.Should().Contain("CollectorEndpoint inválido: invalid-collector-url");
        result.Errors.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("http://tempo:3200", "http://collector:4317")]
    [InlineData("https://tempo.company.com:3200", "https://collector.company.com:4317")]
    [InlineData("http://tempo.monitoring.svc.cluster.local:3200", "http://collector.monitoring.svc.cluster.local:4317")]
    public void TempoAndCollectorEndpoints_ValidCombinations_ShouldBeValid(string tempoUrl, string collectorUrl)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            TempoEndpoint = tempoUrl,
            CollectorEndpoint = collectorUrl
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ObservabilityOptions_DefaultValues_ShouldHaveCorrectTempoAndCollectorEndpoints()
    {
        // Act
        var options = new ObservabilityOptions();

        // Assert
        options.TempoEndpoint.Should().Be("http://localhost:3200");
        options.CollectorEndpoint.Should().Be("http://localhost:4317");
    }

    [Fact]
    public void ObservabilityOptions_CanSetTempoEndpoint()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var customTempoUrl = "http://custom-tempo:3200";

        // Act
        options.TempoEndpoint = customTempoUrl;

        // Assert
        options.TempoEndpoint.Should().Be(customTempoUrl);
    }

    [Fact]
    public void ObservabilityOptions_CanSetCollectorEndpoint()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var customCollectorUrl = "http://custom-collector:4317";

        // Act
        options.CollectorEndpoint = customCollectorUrl;

        // Assert
        options.CollectorEndpoint.Should().Be(customCollectorUrl);
    }

    [Fact]
    public void ObservabilityOptions_CanSetBothEndpoints()
    {
        // Arrange
        var options = new ObservabilityOptions();
        var tempoUrl = "http://tempo:3200";
        var collectorUrl = "http://collector:4317";

        // Act
        options.TempoEndpoint = tempoUrl;
        options.CollectorEndpoint = collectorUrl;

        // Assert
        options.TempoEndpoint.Should().Be(tempoUrl);
        options.CollectorEndpoint.Should().Be(collectorUrl);
    }

    [Theory]
    [InlineData("http://tempo:3200", "http://collector:4317", "http://collector:4317")]
    [InlineData("http://tempo:3200", "", "http://jaeger:4317")]
    [InlineData("http://tempo:3200", null, "http://jaeger:4317")]
    public void EndpointPriority_CollectorShouldHavePriorityOverOtlp(string tempoUrl, string? collectorUrl, string otlpUrl)
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            TempoEndpoint = tempoUrl,
            CollectorEndpoint = collectorUrl ?? "",
            OtlpEndpoint = otlpUrl
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        options.TempoEndpoint.Should().Be(tempoUrl);
        options.CollectorEndpoint.Should().Be(collectorUrl ?? "");
        options.OtlpEndpoint.Should().Be(otlpUrl);
    }
}
