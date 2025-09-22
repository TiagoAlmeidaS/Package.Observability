using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Package.Observability;
using Package.Observability.Exceptions;
using Xunit;

namespace Package.Observability.UnitTests;

/// <summary>
/// Testes unitários para ObservabilityStartupExtensions com Tempo e Collector
/// </summary>
public class TempoCollectorStartupExtensionsTests
{
    [Fact]
    public void AddObservability_WithTempoAndCollectorConfiguration_ShouldConfigureCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:TempoEndpoint"] = "http://tempo:3200",
                ["Observability:CollectorEndpoint"] = "http://collector:4317",
                ["Observability:OtlpEndpoint"] = "http://jaeger:4317"
            })
            .Build();

        // Act
        services.AddObservability(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.ServiceName.Should().Be("TestService");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
        options.CollectorEndpoint.Should().Be("http://collector:4317");
        options.OtlpEndpoint.Should().Be("http://jaeger:4317");
    }

    [Fact]
    public void AddObservability_WithCodeConfiguration_ShouldConfigureTempoAndCollector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "TestService-Code";
            options.EnableTracing = true;
            options.TempoEndpoint = "http://tempo:3200";
            options.CollectorEndpoint = "http://collector:4317";
            options.OtlpEndpoint = "http://jaeger:4317";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.ServiceName.Should().Be("TestService-Code");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
        options.CollectorEndpoint.Should().Be("http://collector:4317");
        options.OtlpEndpoint.Should().Be("http://jaeger:4317");
    }

    [Fact]
    public void AddObservability_WithOnlyCollectorEndpoint_ShouldUseCollector()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:CollectorEndpoint"] = "http://collector:4317",
                ["Observability:OtlpEndpoint"] = "http://jaeger:4317"
            })
            .Build();

        // Act
        services.AddObservability(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.CollectorEndpoint.Should().Be("http://collector:4317");
        options.OtlpEndpoint.Should().Be("http://jaeger:4317");
    }

    [Fact]
    public void AddObservability_WithOnlyOtlpEndpoint_ShouldUseOtlp()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:CollectorEndpoint"] = "",
                ["Observability:OtlpEndpoint"] = "http://jaeger:4317"
            })
            .Build();

        // Act
        services.AddObservability(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.CollectorEndpoint.Should().Be("");
        options.OtlpEndpoint.Should().Be("http://jaeger:4317");
    }

    [Fact]
    public void AddObservability_WithEmptyEndpoints_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:CollectorEndpoint"] = "",
                ["Observability:OtlpEndpoint"] = ""
            })
            .Build();

        // Act & Assert
        var action = () => services.AddObservability(configuration);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddObservability_WithInvalidTempoEndpoint_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:TempoEndpoint"] = "invalid-url"
            })
            .Build();

        // Act & Assert
        var action = () => services.AddObservability(configuration);
        action.Should().Throw<ObservabilityConfigurationException>()
            .WithMessage("*TempoEndpoint inválido*");
    }

    [Fact]
    public void AddObservability_WithInvalidCollectorEndpoint_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:CollectorEndpoint"] = "invalid-url"
            })
            .Build();

        // Act & Assert
        var action = () => services.AddObservability(configuration);
        action.Should().Throw<ObservabilityConfigurationException>()
            .WithMessage("*CollectorEndpoint inválido*");
    }

    [Fact]
    public void AddObservability_WithBothInvalidEndpoints_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "true",
                ["Observability:TempoEndpoint"] = "invalid-tempo-url",
                ["Observability:CollectorEndpoint"] = "invalid-collector-url"
            })
            .Build();

        // Act & Assert
        var action = () => services.AddObservability(configuration);
        action.Should().Throw<ObservabilityConfigurationException>()
            .WithMessage("*TempoEndpoint inválido*");
    }

    [Fact]
    public void AddObservability_WithTracingDisabled_ShouldNotValidateEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableTracing"] = "false",
                ["Observability:TempoEndpoint"] = "invalid-url",
                ["Observability:CollectorEndpoint"] = "invalid-url"
            })
            .Build();

        // Act & Assert
        var action = () => services.AddObservability(configuration);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddObservability_WithCustomSectionName_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CustomObservability:ServiceName"] = "TestService",
                ["CustomObservability:EnableTracing"] = "true",
                ["CustomObservability:TempoEndpoint"] = "http://tempo:3200",
                ["CustomObservability:CollectorEndpoint"] = "http://collector:4317"
            })
            .Build();

        // Act
        services.AddObservability(configuration, "CustomObservability");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.ServiceName.Should().Be("TestService");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
        options.CollectorEndpoint.Should().Be("http://collector:4317");
    }

    [Fact]
    public void AddObservability_WithCodeConfiguration_ShouldIncludeAllProperties()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "TestService";
            options.EnableTracing = true;
            options.TempoEndpoint = "http://tempo:3200";
            options.CollectorEndpoint = "http://collector:4317";
            options.OtlpEndpoint = "http://jaeger:4317";
            options.AdditionalLabels.Add("environment", "test");
            options.AdditionalLabels.Add("component", "tempo-collector");
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        options.ServiceName.Should().Be("TestService");
        options.TempoEndpoint.Should().Be("http://tempo:3200");
        options.CollectorEndpoint.Should().Be("http://collector:4317");
        options.OtlpEndpoint.Should().Be("http://jaeger:4317");
        options.AdditionalLabels.Should().ContainKey("environment");
        options.AdditionalLabels.Should().ContainKey("component");
    }

    [Fact]
    public void AddObservability_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddObservability((IConfiguration)null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddObservability_WithNullConfigureOptions_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddObservability((Action<ObservabilityOptions>)null!);
        action.Should().Throw<ArgumentNullException>();
    }
}
