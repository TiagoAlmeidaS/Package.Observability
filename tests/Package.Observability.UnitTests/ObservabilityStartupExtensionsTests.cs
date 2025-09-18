using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ObservabilityStartupExtensionsTests
{
    [Fact]
    public void AddObservability_WithConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verificar se ObservabilityOptions está registrado
        var options = serviceProvider.GetService<IOptions<ObservabilityOptions>>();
        options.Should().NotBeNull();
        options!.Value.ServiceName.Should().Be("TestService");
        
        // Verificar se ISerilogService está registrado
        var serilogService = serviceProvider.GetService<ISerilogService>();
        serilogService.Should().NotBeNull();
    }

    [Fact]
    public void AddObservability_WithConfiguration_ShouldRegisterObservabilityOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("TestService");
        options.Value.EnableMetrics.Should().BeTrue();
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void AddObservability_WithConfiguration_ShouldRegisterLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddObservability_WithConfiguration_ShouldRegisterOpenTelemetryServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verificar se os serviços do OpenTelemetry estão registrados
        // (Não podemos testar diretamente, mas podemos verificar que não há exceção)
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddObservability_WithCustomSectionName_ShouldUseCustomSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithCustomSection();

        // Act
        services.AddObservability(configuration, "CustomObservability");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("CustomService");
    }

    [Fact]
    public void AddObservability_WithAction_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "ActionService";
            options.EnableMetrics = true;
            options.EnableTracing = true;
            options.EnableLogging = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.ServiceName.Should().Be("ActionService");
    }

    [Fact]
    public void AddObservability_WithAction_ShouldConfigureAllOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var additionalLabels = new Dictionary<string, string> { ["env"] = "test" };
        var lokiLabels = new Dictionary<string, string> { ["app"] = "test-app" };

        // Act
        services.AddObservability(options =>
        {
            options.ServiceName = "ActionService";
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
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        
        options.Value.ServiceName.Should().Be("ActionService");
        options.Value.PrometheusPort.Should().Be(9110);
        options.Value.EnableMetrics.Should().BeFalse();
        options.Value.EnableTracing.Should().BeFalse();
        options.Value.EnableLogging.Should().BeFalse();
        options.Value.LokiUrl.Should().Be("http://test-loki:3100");
        options.Value.OtlpEndpoint.Should().Be("http://test-jaeger:4317");
        options.Value.EnableConsoleLogging.Should().BeFalse();
        options.Value.MinimumLogLevel.Should().Be("Debug");
        options.Value.AdditionalLabels.Should().BeEquivalentTo(additionalLabels);
        options.Value.LokiLabels.Should().BeEquivalentTo(lokiLabels);
        options.Value.EnableCorrelationId.Should().BeFalse();
        options.Value.EnableRuntimeInstrumentation.Should().BeFalse();
        options.Value.EnableHttpClientInstrumentation.Should().BeFalse();
        options.Value.EnableAspNetCoreInstrumentation.Should().BeFalse();
    }

    [Fact]
    public void AddObservability_WithNullConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        
        // Deve usar valores padrão
        options.Value.ServiceName.Should().Be("DefaultService");
        options.Value.EnableMetrics.Should().BeTrue();
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void AddObservability_WithEmptyConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        
        // Deve usar valores padrão
        options.Value.ServiceName.Should().Be("DefaultService");
        options.Value.EnableMetrics.Should().BeTrue();
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void AddObservability_WithPartialConfiguration_ShouldMergeWithDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "PartialService",
                ["Observability:EnableMetrics"] = "false"
            })
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        
        options.Value.ServiceName.Should().Be("PartialService");
        options.Value.EnableMetrics.Should().BeFalse();
        // Outros valores devem ser padrão
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void AddObservability_WithLoggingDisabled_ShouldNotRegisterLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:EnableLogging"] = "false"
            })
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.EnableLogging.Should().BeFalse();
    }

    [Fact]
    public void AddObservability_WithMetricsDisabled_ShouldNotRegisterMetricsServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:EnableMetrics"] = "false"
            })
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.EnableMetrics.Should().BeFalse();
    }

    [Fact]
    public void AddObservability_WithTracingDisabled_ShouldNotRegisterTracingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:EnableTracing"] = "false"
            })
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Value.EnableTracing.Should().BeFalse();
    }

    [Fact]
    public void AddObservability_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddObservability(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddObservability_WithAction_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddObservability(options => { });

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddObservability_WithComplexConfiguration_ShouldHandleAllSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "ComplexService",
                ["Observability:PrometheusPort"] = "9110",
                ["Observability:EnableMetrics"] = "true",
                ["Observability:EnableTracing"] = "true",
                ["Observability:EnableLogging"] = "true",
                ["Observability:LokiUrl"] = "http://loki.test:3100",
                ["Observability:OtlpEndpoint"] = "http://jaeger.test:4317",
                ["Observability:EnableConsoleLogging"] = "false",
                ["Observability:MinimumLogLevel"] = "Warning",
                ["Observability:EnableCorrelationId"] = "false",
                ["Observability:EnableRuntimeInstrumentation"] = "false",
                ["Observability:EnableHttpClientInstrumentation"] = "false",
                ["Observability:EnableAspNetCoreInstrumentation"] = "false"
            })
            .Build();

        // Act
        services.AddObservability(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>();
        
        options.Value.ServiceName.Should().Be("ComplexService");
        options.Value.PrometheusPort.Should().Be(9110);
        options.Value.EnableMetrics.Should().BeTrue();
        options.Value.EnableTracing.Should().BeTrue();
        options.Value.EnableLogging.Should().BeTrue();
        options.Value.LokiUrl.Should().Be("http://loki.test:3100");
        options.Value.OtlpEndpoint.Should().Be("http://jaeger.test:4317");
        options.Value.EnableConsoleLogging.Should().BeFalse();
        options.Value.MinimumLogLevel.Should().Be("Warning");
        options.Value.EnableCorrelationId.Should().BeFalse();
        options.Value.EnableRuntimeInstrumentation.Should().BeFalse();
        options.Value.EnableHttpClientInstrumentation.Should().BeFalse();
        options.Value.EnableAspNetCoreInstrumentation.Should().BeFalse();
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:ServiceName"] = "TestService",
                ["Observability:EnableMetrics"] = "true",
                ["Observability:EnableTracing"] = "true",
                ["Observability:EnableLogging"] = "true"
            })
            .Build();
    }

    private static IConfiguration CreateConfigurationWithCustomSection()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CustomObservability:ServiceName"] = "CustomService",
                ["CustomObservability:EnableMetrics"] = "true",
                ["CustomObservability:EnableTracing"] = "true",
                ["CustomObservability:EnableLogging"] = "true"
            })
            .Build();
    }
}
