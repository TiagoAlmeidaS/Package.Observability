using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Package.Observability.Tests;

[TestClass]
public class ObservabilityStartupExtensionsTests
{
    [TestMethod]
    public void AddObservability_WithConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verificar se IOptions<ObservabilityOptions> foi registrado
        var options = serviceProvider.GetService<IOptions<ObservabilityOptions>>();
        options.Should().NotBeNull();
        options!.Value.ServiceName.Should().Be("TestService");
    }

    [TestMethod]
    public void AddObservability_WithActionConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "ActionConfiguredService";
            options.PrometheusPort = 8080;
            options.EnableMetrics = false;
            options.EnableTracing = false;
            options.EnableLogging = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("ActionConfiguredService");
        options.Value.PrometheusPort.Should().Be(8080);
        options.Value.EnableMetrics.Should().BeFalse();
        options.Value.EnableTracing.Should().BeFalse();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [TestMethod]
    public void AddObservability_WithCustomSectionName_ShouldWork()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            ["CustomObservability:ServiceName"] = "CustomSectionService",
            ["CustomObservability:PrometheusPort"] = "7070"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddObservability(configuration, "CustomObservability");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("CustomSectionService");
        options.Value.PrometheusPort.Should().Be(7070);
    }

    [TestMethod]
    public void AddObservability_WithEmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("DefaultService");
        options.Value.PrometheusPort.Should().Be(9090);
        options.Value.EnableMetrics.Should().BeTrue();
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [TestMethod]
    public void AddObservability_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act & Assert
        var act = () =>
        {
            services.AddObservability(configuration);
            services.AddObservability(configuration);
            services.AddObservability(configuration);
        };

        act.Should().NotThrow();
    }

    [TestMethod]
    public void AddObservability_WithActionConfiguration_ShouldSetAllProperties()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "CompleteTestService";
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
            options.AdditionalLabels.Add("test-key", "test-value");
            options.LokiLabels.Add("loki-key", "loki-value");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("CompleteTestService");
        options.Value.PrometheusPort.Should().Be(9999);
        options.Value.EnableMetrics.Should().BeFalse();
        options.Value.EnableTracing.Should().BeFalse();
        options.Value.EnableLogging.Should().BeFalse();
        options.Value.LokiUrl.Should().Be("http://test-loki:3100");
        options.Value.OtlpEndpoint.Should().Be("http://test-jaeger:4317");
        options.Value.EnableConsoleLogging.Should().BeFalse();
        options.Value.MinimumLogLevel.Should().Be("Debug");
        options.Value.EnableCorrelationId.Should().BeFalse();
        options.Value.EnableRuntimeInstrumentation.Should().BeFalse();
        options.Value.EnableHttpClientInstrumentation.Should().BeFalse();
        options.Value.EnableAspNetCoreInstrumentation.Should().BeFalse();
        options.Value.AdditionalLabels.Should().ContainKey("test-key").WhoseValue.Should().Be("test-value");
        options.Value.LokiLabels.Should().ContainKey("loki-key").WhoseValue.Should().Be("loki-value");
    }

    [TestMethod]
    public void AddObservability_ShouldRegisterLoggingServices_WhenLoggingEnabled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "LoggingTestService";
            options.EnableLogging = true;
            options.EnableMetrics = false;
            options.EnableTracing = false;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verificar se serviços de logging foram registrados
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();

        var logger = serviceProvider.GetService<ILogger<ObservabilityStartupExtensionsTests>>();
        logger.Should().NotBeNull();
    }

    [TestMethod]
    public void AddObservability_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? nullConfiguration = null;

        // Act & Assert
        var act = () => services.AddObservability(nullConfiguration!);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void AddObservability_WithNullAction_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<ObservabilityOptions>? nullAction = null;

        // Act & Assert
        var act = () => services.AddObservability(nullAction!);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void AddObservability_WithNullServices_ShouldThrow()
    {
        // Arrange
        IServiceCollection? nullServices = null;
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var act = () => nullServices!.AddObservability(configuration);
        act.Should().Throw<ArgumentNullException>();
    }
}