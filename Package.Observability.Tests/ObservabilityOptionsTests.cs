using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Package.Observability.Tests;

[TestClass]
public class ObservabilityOptionsTests
{
    [TestMethod]
    public void ObservabilityOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
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
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRuntimeInstrumentation.Should().BeTrue();
        options.EnableHttpClientInstrumentation.Should().BeTrue();
        options.EnableAspNetCoreInstrumentation.Should().BeTrue();
        options.AdditionalLabels.Should().NotBeNull().And.BeEmpty();
        options.LokiLabels.Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void ObservabilityOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new ObservabilityOptions();

        // Act
        options.ServiceName = "TestService";
        options.PrometheusPort = 9999;
        options.EnableMetrics = false;
        options.EnableTracing = false;
        options.EnableLogging = false;
        options.LokiUrl = "http://test-loki:3100";
        options.OtlpEndpoint = "http://test-jaeger:4317";
        options.EnableConsoleLogging = false;
        options.MinimumLogLevel = "Debug";
        options.EnableCorrelationId = false;
        options.EnableRuntimeInstrumentation = false;
        options.EnableHttpClientInstrumentation = false;
        options.EnableAspNetCoreInstrumentation = false;
        options.AdditionalLabels.Add("test", "value");
        options.LokiLabels.Add("loki", "test");

        // Assert
        options.ServiceName.Should().Be("TestService");
        options.PrometheusPort.Should().Be(9999);
        options.EnableMetrics.Should().BeFalse();
        options.EnableTracing.Should().BeFalse();
        options.EnableLogging.Should().BeFalse();
        options.LokiUrl.Should().Be("http://test-loki:3100");
        options.OtlpEndpoint.Should().Be("http://test-jaeger:4317");
        options.EnableConsoleLogging.Should().BeFalse();
        options.MinimumLogLevel.Should().Be("Debug");
        options.EnableCorrelationId.Should().BeFalse();
        options.EnableRuntimeInstrumentation.Should().BeFalse();
        options.EnableHttpClientInstrumentation.Should().BeFalse();
        options.EnableAspNetCoreInstrumentation.Should().BeFalse();
        options.AdditionalLabels.Should().ContainKey("test").WhoseValue.Should().Be("value");
        options.LokiLabels.Should().ContainKey("loki").WhoseValue.Should().Be("test");
    }

    [TestMethod]
    public void ObservabilityOptions_BindFromConfiguration_ShouldWork()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        var services = new ServiceCollection();
        services.Configure<ObservabilityOptions>(configuration.GetSection("Observability"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

        // Assert
        options.ServiceName.Should().Be("TestService");
        options.PrometheusPort.Should().Be(9999);
        options.EnableMetrics.Should().BeTrue();
        options.EnableTracing.Should().BeTrue();
        options.EnableLogging.Should().BeTrue();
        options.LokiUrl.Should().Be("http://test-loki:3100");
        options.OtlpEndpoint.Should().Be("http://test-jaeger:4317");
        options.EnableConsoleLogging.Should().BeFalse();
        options.MinimumLogLevel.Should().Be("Information");
        options.EnableCorrelationId.Should().BeTrue();
        options.AdditionalLabels.Should().ContainKey("environment").WhoseValue.Should().Be("test");
        options.AdditionalLabels.Should().ContainKey("version").WhoseValue.Should().Be("1.0.0-test");
        options.LokiLabels.Should().ContainKey("app").WhoseValue.Should().Be("test-app");
        options.LokiLabels.Should().ContainKey("component").WhoseValue.Should().Be("test");
    }

    [TestMethod]
    public void ObservabilityOptions_BindFromInMemoryConfiguration_ShouldWork()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            ["Observability:ServiceName"] = "InMemoryTestService",
            ["Observability:PrometheusPort"] = "8080",
            ["Observability:EnableMetrics"] = "false",
            ["Observability:EnableTracing"] = "false",
            ["Observability:EnableLogging"] = "true",
            ["Observability:LokiUrl"] = "http://memory-loki:3100",
            ["Observability:OtlpEndpoint"] = "http://memory-jaeger:4317",
            ["Observability:EnableConsoleLogging"] = "true",
            ["Observability:MinimumLogLevel"] = "Debug",
            ["Observability:EnableCorrelationId"] = "false"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<ObservabilityOptions>(configuration.GetSection("Observability"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

        // Assert
        options.ServiceName.Should().Be("InMemoryTestService");
        options.PrometheusPort.Should().Be(8080);
        options.EnableMetrics.Should().BeFalse();
        options.EnableTracing.Should().BeFalse();
        options.EnableLogging.Should().BeTrue();
        options.LokiUrl.Should().Be("http://memory-loki:3100");
        options.OtlpEndpoint.Should().Be("http://memory-jaeger:4317");
        options.EnableConsoleLogging.Should().BeTrue();
        options.MinimumLogLevel.Should().Be("Debug");
        options.EnableCorrelationId.Should().BeFalse();
    }

    [TestMethod]
    public void ObservabilityOptions_EmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.Configure<ObservabilityOptions>(configuration.GetSection("Observability"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

        // Assert
        options.ServiceName.Should().Be("DefaultService");
        options.PrometheusPort.Should().Be(9090);
        options.EnableMetrics.Should().BeTrue();
        options.EnableTracing.Should().BeTrue();
        options.EnableLogging.Should().BeTrue();
    }

    [TestMethod]
    public void ObservabilityOptions_PartialConfiguration_ShouldMergeWithDefaults()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            ["Observability:ServiceName"] = "PartialTestService",
            ["Observability:EnableMetrics"] = "false"
            // Outras propriedades não definidas devem usar valores padrão
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<ObservabilityOptions>(configuration.GetSection("Observability"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

        // Assert
        options.ServiceName.Should().Be("PartialTestService"); // Configurado
        options.EnableMetrics.Should().BeFalse(); // Configurado
        options.PrometheusPort.Should().Be(9090); // Padrão
        options.EnableTracing.Should().BeTrue(); // Padrão
        options.EnableLogging.Should().BeTrue(); // Padrão
        options.LokiUrl.Should().Be("http://localhost:3100"); // Padrão
    }
}