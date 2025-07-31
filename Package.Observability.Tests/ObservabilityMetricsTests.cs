using FluentAssertions;
using System.Diagnostics.Metrics;

namespace Package.Observability.Tests;

[TestClass]
public class ObservabilityMetricsTests
{
    [TestCleanup]
    public void Cleanup()
    {
        // Limpar Meters criados durante os testes
        ObservabilityMetrics.DisposeAll();
    }

    [TestMethod]
    public void GetOrCreateMeter_WithServiceName_ShouldReturnMeter()
    {
        // Arrange
        const string serviceName = "TestService";

        // Act
        var meter = ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be(serviceName);
        meter.Version.Should().Be("1.0.0");
    }

    [TestMethod]
    public void GetOrCreateMeter_WithServiceNameAndVersion_ShouldReturnMeter()
    {
        // Arrange
        const string serviceName = "TestService";
        const string version = "2.1.0";

        // Act
        var meter = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);

        // Assert
        meter.Should().NotBeNull();
        meter.Name.Should().Be(serviceName);
        meter.Version.Should().Be(version);
    }

    [TestMethod]
    public void GetOrCreateMeter_WithSameParameters_ShouldReturnSameInstance()
    {
        // Arrange
        const string serviceName = "TestService";
        const string version = "1.0.0";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName, version);

        // Assert
        meter1.Should().BeSameAs(meter2);
    }

    [TestMethod]
    public void GetOrCreateMeter_WithDifferentServiceNames_ShouldReturnDifferentInstances()
    {
        // Arrange
        const string serviceName1 = "TestService1";
        const string serviceName2 = "TestService2";

        // Act
        var meter1 = ObservabilityMetrics.GetOrCreateMeter(serviceName1);
        var meter2 = ObservabilityMetrics.GetOrCreateMeter(serviceName2);

        // Assert
        meter1.Should().NotBeSameAs(meter2);
        meter1.Name.Should().Be(serviceName1);
        meter2.Name.Should().Be(serviceName2);
    }

    [TestMethod]
    public void GetOrCreateMeter_WithNullServiceName_ShouldThrow()
    {
        // Arrange
        string? nullServiceName = null;

        // Act & Assert
        var act = () => ObservabilityMetrics.GetOrCreateMeter(nullServiceName!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Service name cannot be null or empty*");
    }

    [TestMethod]
    public void GetOrCreateMeter_WithEmptyServiceName_ShouldThrow()
    {
        // Arrange
        const string emptyServiceName = "";

        // Act & Assert
        var act = () => ObservabilityMetrics.GetOrCreateMeter(emptyServiceName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Service name cannot be null or empty*");
    }

    [TestMethod]
    public void CreateCounter_WithValidParameters_ShouldReturnCounter()
    {
        // Arrange
        const string serviceName = "TestService";
        const string counterName = "test_counter";
        const string unit = "count";
        const string description = "Test counter description";

        // Act
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, counterName, unit, description);

        // Assert
        counter.Should().NotBeNull();
        counter.Name.Should().Be(counterName);
        counter.Unit.Should().Be(unit);
        counter.Description.Should().Be(description);
    }

    [TestMethod]
    public void CreateCounter_WithMinimalParameters_ShouldReturnCounter()
    {
        // Arrange
        const string serviceName = "TestService";
        const string counterName = "minimal_counter";

        // Act
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, counterName);

        // Assert
        counter.Should().NotBeNull();
        counter.Name.Should().Be(counterName);
    }

    [TestMethod]
    public void CreateHistogram_WithValidParameters_ShouldReturnHistogram()
    {
        // Arrange
        const string serviceName = "TestService";
        const string histogramName = "test_histogram";
        const string unit = "ms";
        const string description = "Test histogram description";

        // Act
        var histogram = ObservabilityMetrics.CreateHistogram<double>(serviceName, histogramName, unit, description);

        // Assert
        histogram.Should().NotBeNull();
        histogram.Name.Should().Be(histogramName);
        histogram.Unit.Should().Be(unit);
        histogram.Description.Should().Be(description);
    }

    [TestMethod]
    public void CreateObservableGauge_WithValidParameters_ShouldReturnGauge()
    {
        // Arrange
        const string serviceName = "TestService";
        const string gaugeName = "test_gauge";
        const string unit = "count";
        const string description = "Test gauge description";
        static int observeValue() => 42;

        // Act
        var gauge = ObservabilityMetrics.CreateObservableGauge<int>(serviceName, gaugeName, observeValue, unit, description);

        // Assert
        gauge.Should().NotBeNull();
        gauge.Name.Should().Be(gaugeName);
        gauge.Unit.Should().Be(unit);
        gauge.Description.Should().Be(description);
    }

    [TestMethod]
    public void CreateUpDownCounter_WithValidParameters_ShouldReturnUpDownCounter()
    {
        // Arrange
        const string serviceName = "TestService";
        const string counterName = "test_updown_counter";
        const string unit = "items";
        const string description = "Test up-down counter description";

        // Act
        var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>(serviceName, counterName, unit, description);

        // Assert
        upDownCounter.Should().NotBeNull();
        upDownCounter.Name.Should().Be(counterName);
        upDownCounter.Unit.Should().Be(unit);
        upDownCounter.Description.Should().Be(description);
    }

    [TestMethod]
    public void CreateCounter_WithDifferentTypes_ShouldWork()
    {
        // Arrange
        const string serviceName = "TestService";

        // Act & Assert
        var intCounter = ObservabilityMetrics.CreateCounter<int>(serviceName, "int_counter");
        intCounter.Should().NotBeNull();

        var longCounter = ObservabilityMetrics.CreateCounter<long>(serviceName, "long_counter");
        longCounter.Should().NotBeNull();

        var floatCounter = ObservabilityMetrics.CreateCounter<float>(serviceName, "float_counter");
        floatCounter.Should().NotBeNull();

        var doubleCounter = ObservabilityMetrics.CreateCounter<double>(serviceName, "double_counter");
        doubleCounter.Should().NotBeNull();
    }

    [TestMethod]
    public void CreateHistogram_WithDifferentTypes_ShouldWork()
    {
        // Arrange
        const string serviceName = "TestService";

        // Act & Assert
        var intHistogram = ObservabilityMetrics.CreateHistogram<int>(serviceName, "int_histogram");
        intHistogram.Should().NotBeNull();

        var longHistogram = ObservabilityMetrics.CreateHistogram<long>(serviceName, "long_histogram");
        longHistogram.Should().NotBeNull();

        var floatHistogram = ObservabilityMetrics.CreateHistogram<float>(serviceName, "float_histogram");
        floatHistogram.Should().NotBeNull();

        var doubleHistogram = ObservabilityMetrics.CreateHistogram<double>(serviceName, "double_histogram");
        doubleHistogram.Should().NotBeNull();
    }

    [TestMethod]
    public void DisposeAll_ShouldDisposeAllMeters()
    {
        // Arrange
        var meter1 = ObservabilityMetrics.GetOrCreateMeter("Service1");
        var meter2 = ObservabilityMetrics.GetOrCreateMeter("Service2");

        // Act
        ObservabilityMetrics.DisposeAll();

        // Assert
        // Após DisposeAll, novos Meters devem ser criados
        var newMeter1 = ObservabilityMetrics.GetOrCreateMeter("Service1");
        newMeter1.Should().NotBeSameAs(meter1);
    }

    [TestMethod]
    public void DisposeAll_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        ObservabilityMetrics.GetOrCreateMeter("Service1");
        ObservabilityMetrics.GetOrCreateMeter("Service2");

        // Act & Assert
        var act = () =>
        {
            ObservabilityMetrics.DisposeAll();
            ObservabilityMetrics.DisposeAll();
            ObservabilityMetrics.DisposeAll();
        };

        act.Should().NotThrow();
    }

    [TestMethod]
    public void CreateMetrics_FromSameMeter_ShouldUseSameMeterInstance()
    {
        // Arrange
        const string serviceName = "TestService";

        // Act
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, "counter1");
        var histogram = ObservabilityMetrics.CreateHistogram<double>(serviceName, "histogram1");
        var upDownCounter = ObservabilityMetrics.CreateUpDownCounter<int>(serviceName, "updown1");

        // Assert
        // Todos devem usar o mesmo Meter interno
        counter.Meter.Should().BeSameAs(histogram.Meter);
        histogram.Meter.Should().BeSameAs(upDownCounter.Meter);
    }

    [TestMethod]
    public void GetOrCreateMeter_ThreadSafety_ShouldHandleConcurrentAccess()
    {
        // Arrange
        const string serviceName = "ConcurrentTestService";
        const int threadCount = 10;
        var meters = new Meter[threadCount];
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                meters[index] = ObservabilityMetrics.GetOrCreateMeter(serviceName);
            });
        }

        Task.WaitAll(tasks);

        // Assert
        // Todas as instâncias devem ser a mesma (thread-safe singleton)
        for (int i = 1; i < threadCount; i++)
        {
            meters[i].Should().BeSameAs(meters[0]);
        }
    }

    [TestMethod]
    public void CreateMetrics_WithNullServiceName_ShouldThrow()
    {
        // Arrange
        string? nullServiceName = null;

        // Act & Assert
        var act1 = () => ObservabilityMetrics.CreateCounter<int>(nullServiceName!, "counter");
        act1.Should().Throw<ArgumentException>();

        var act2 = () => ObservabilityMetrics.CreateHistogram<double>(nullServiceName!, "histogram");
        act2.Should().Throw<ArgumentException>();

        var act3 = () => ObservabilityMetrics.CreateUpDownCounter<int>(nullServiceName!, "updown");
        act3.Should().Throw<ArgumentException>();

        var act4 = () => ObservabilityMetrics.CreateObservableGauge<int>(nullServiceName!, "gauge", () => 1);
        act4.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void CreateMetrics_AfterDisposeAll_ShouldCreateNewMeterInstances()
    {
        // Arrange
        const string serviceName = "TestService";
        var originalMeter = ObservabilityMetrics.GetOrCreateMeter(serviceName);

        // Act
        ObservabilityMetrics.DisposeAll();
        var counter = ObservabilityMetrics.CreateCounter<int>(serviceName, "new_counter");

        // Assert
        counter.Meter.Should().NotBeSameAs(originalMeter);
        counter.Meter.Name.Should().Be(serviceName);
    }
}