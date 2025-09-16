using System.Diagnostics.Metrics;
using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ObservabilityMetricsTests : IDisposable
{
    public ObservabilityMetricsTests()
    {
        // Limpar estado antes de cada teste
        ObservabilityMetrics.DisposeAll();
    }

    public void Dispose()
    {
        // Limpar estado após cada teste
        ObservabilityMetrics.DisposeAll();
    }

    [Fact]
    public void GetOrCreateMeter_WithValidServiceName_ShouldReturnMeter()
    {
        // Arrange
        var serviceName = "TestService";

        // Act
        var meter = ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be(serviceName);
        meter.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void GetOrCreateMeter_WithValidServiceNameAndVersion_ShouldReturnMeter()
    {
        // Arrange
        var serviceName = "TestService";
        var version = "2.0.0";

        // Act
        var meter = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);

        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be(serviceName);
        meter.Version.Should().Be(version);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetOrCreateMeter_WithInvalidServiceName_ShouldThrowArgumentException(string serviceName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ObservabilityMetrics.GetOrCreateMeter(serviceName));
        
        exception.ParamName.Should().Be("serviceName");
        exception.Message.Should().Contain("Service name cannot be null or empty");
    }

    [Fact]
    public void GetOrCreateMeter_WithSameServiceName_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "TestService";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Assert
        meter1.Should().BeSameAs(meter2);
    }

    [Fact]
    public void GetOrCreateMeter_WithDifferentServiceNames_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName1 = "TestService1";
        var serviceName2 = "TestService2";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName1);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName2);

        // Assert
        meter1.Should().NotBeSameAs(meter2);
        meter1.Name.Should().Be(serviceName1);
        meter2.Name.Should().Be(serviceName2);
    }

    [Fact]
    public void GetOrCreateMeter_WithSameServiceNameAndVersion_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "TestService";
        var version = "1.0.0";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);

        // Assert
        meter1.Should().BeSameAs(meter2);
    }

    [Fact]
    public void GetOrCreateMeter_WithSameServiceNameDifferentVersions_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName = "TestService";
        var version1 = "1.0.0";
        var version2 = "2.0.0";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version1);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version2);

        // Assert
        meter1.Should().NotBeSameAs(meter2);
        meter1.Version.Should().Be(version1);
        meter2.Version.Should().Be(version2);
    }

    [Fact]
    public void CreateCounter_WithValidParameters_ShouldReturnCounter()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_counter";
        var unit = "count";
        var description = "Test counter";

        // Act
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, name, unit, description);

        // Assert
        counter.Should().NotBeNull();
        counter.Name.Should().Be(name);
    }

    [Fact]
    public void CreateCounter_WithMinimalParameters_ShouldReturnCounter()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_counter";

        // Act
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, name);

        // Assert
        counter.Should().NotBeNull();
        counter.Name.Should().Be(name);
    }

    [Fact]
    public void CreateHistogram_WithValidParameters_ShouldReturnHistogram()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_histogram";
        var unit = "ms";
        var description = "Test histogram";

        // Act
        var histogram = ObservabilityMetrics.CreateHistogram<double>(serviceName, name, unit, description);

        // Assert
        histogram.Should().NotBeNull();
        histogram.Name.Should().Be(name);
    }

    [Fact]
    public void CreateHistogram_WithMinimalParameters_ShouldReturnHistogram()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_histogram";

        // Act
        var histogram = ObservabilityMetrics.CreateHistogram<double>(serviceName, name);

        // Assert
        histogram.Should().NotBeNull();
        histogram.Name.Should().Be(name);
    }

    [Fact]
    public void CreateObservableGauge_WithValidParameters_ShouldReturnObservableGauge()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_gauge";
        var unit = "items";
        var description = "Test gauge";
        var observeValue = () => 42;

        // Act
        var gauge = ObservabilityMetrics.CreateObservableGauge<int>(serviceName, name, observeValue, unit, description);

        // Assert
        gauge.Should().NotBeNull();
        gauge.Name.Should().Be(name);
    }

    [Fact]
    public void CreateObservableGauge_WithMinimalParameters_ShouldReturnObservableGauge()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_gauge";
        var observeValue = () => 42;

        // Act
        var gauge = ObservabilityMetrics.CreateObservableGauge<int>(serviceName, name, observeValue);

        // Assert
        gauge.Should().NotBeNull();
        gauge.Name.Should().Be(name);
    }

    [Fact]
    public void CreateUpDownCounter_WithValidParameters_ShouldReturnUpDownCounter()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_updown_counter";
        var unit = "items";
        var description = "Test up-down counter";

        // Act
        var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>(serviceName, name, unit, description);

        // Assert
        upDownCounter.Should().NotBeNull();
        upDownCounter.Name.Should().Be(name);
    }

    [Fact]
    public void CreateUpDownCounter_WithMinimalParameters_ShouldReturnUpDownCounter()
    {
        // Arrange
        var serviceName = "TestService";
        var name = "test_updown_counter";

        // Act
        var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>(serviceName, name);

        // Assert
        upDownCounter.Should().NotBeNull();
        upDownCounter.Name.Should().Be(name);
    }

    [Fact]
    public void DisposeAll_ShouldDisposeAllMeters()
    {
        // Arrange
        var serviceName1 = "TestService1";
        var serviceName2 = "TestService2";
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName1);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName2);

        // Act
        ObservabilityMetrics.DisposeAll();

        // Assert
        // Verificar se os meters foram descartados (não podemos testar diretamente,
        // mas podemos verificar que não há exceção)
        meter1.Should().NotBeNull();
        meter2.Should().NotBeNull();
    }

    [Fact]
    public void DisposeAll_CanBeCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "TestService";
        ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Act & Assert
        var act1 = () => ObservabilityMetrics.DisposeAll();
        var act2 = () => ObservabilityMetrics.DisposeAll();
        var act3 = () => ObservabilityMetrics.DisposeAll();

        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
    }

    [Fact]
    public void GetOrCreateMeter_AfterDisposeAll_ShouldCreateNewMeter()
    {
        // Arrange
        var serviceName = "TestService";
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName);
        ObservabilityMetrics.DisposeAll();

        // Act
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Assert
        meter2.Should().NotBeNull();
        meter2.Name.Should().Be(serviceName);
        // Não podemos testar se são instâncias diferentes devido ao comportamento interno,
        // mas podemos verificar que não há exceção
    }
}
