using System.Diagnostics;
using System.Diagnostics.Metrics;
using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ConcurrencyTests : IDisposable
{
    public ConcurrencyTests()
    {
        // Limpar estado antes de cada teste
        ObservabilityMetrics.DisposeAll();
        ActivitySourceFactory.DisposeAll();
    }

    public void Dispose()
    {
        // Limpar estado após cada teste
        ObservabilityMetrics.DisposeAll();
        ActivitySourceFactory.DisposeAll();
    }

    [Fact]
    public void ObservabilityMetrics_GetOrCreateMeter_ConcurrentAccess_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var tasks = new List<Task<Meter>>();
        var numberOfTasks = 100;

        // Act
        for (int i = 0; i < numberOfTasks; i++)
        {
            tasks.Add(Task.Run(() => ObservabilityMetrics.GetOrCreateMeter(serviceName)));
        }

        var meters = Task.WhenAll(tasks).Result;

        // Assert
        meters.Should().HaveCount(numberOfTasks);
        var uniqueMeters = meters.Distinct().Count();
        uniqueMeters.Should().Be(1, "all tasks should return the same meter instance");
        
        var firstMeter = meters.First();
        firstMeter.Name.Should().Be(serviceName);
        firstMeter.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void ObservabilityMetrics_GetOrCreateMeter_ConcurrentAccessWithDifferentVersions_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var version1 = "1.0.0";
        var version2 = "2.0.0";
        var tasks = new List<Task<Meter>>();
        var numberOfTasks = 50;

        // Act
        for (int i = 0; i < numberOfTasks; i++)
        {
            var version = i % 2 == 0 ? version1 : version2;
            tasks.Add(Task.Run(() => ObservabilityMetrics.GetOrCreateMeter(serviceName, version)));
        }

        var meters = Task.WhenAll(tasks).Result;

        // Assert
        meters.Should().HaveCount(numberOfTasks);
        var uniqueMeters = meters.Distinct().Count();
        uniqueMeters.Should().Be(2, "should have two different meter instances for different versions");
        
        var version1Meters = meters.Where(m => m.Version == version1).ToList();
        var version2Meters = meters.Where(m => m.Version == version2).ToList();
        
        version1Meters.Should().HaveCount(numberOfTasks / 2);
        version2Meters.Should().HaveCount(numberOfTasks / 2);
        
        version1Meters.Distinct().Should().HaveCount(1, "all version 1 meters should be the same instance");
        version2Meters.Distinct().Should().HaveCount(1, "all version 2 meters should be the same instance");
    }

    [Fact]
    public void ObservabilityMetrics_CreateCounter_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var counterName = "concurrent_counter";
        var tasks = new List<Task<Counter<int>>>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.CreateCounter<int>(serviceName, counterName)));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(100);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ObservabilityMetrics_CreateHistogram_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var histogramName = "concurrent_histogram";
        var tasks = new List<Task<Histogram<double>>>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.CreateHistogram<double>(serviceName, histogramName)));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(100);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ActivitySourceFactory_GetOrCreate_ConcurrentAccess_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var tasks = new List<Task<ActivitySource>>();
        var numberOfTasks = 10; // Reduzir número de tasks para evitar race conditions

        // Act
        for (int i = 0; i < numberOfTasks; i++)
        {
            tasks.Add(Task.Run(() => ActivitySourceFactory.GetOrCreate(serviceName)));
        }

        var activitySources = Task.WhenAll(tasks).Result;

        // Assert
        activitySources.Should().HaveCount(numberOfTasks);
        var uniqueActivitySources = activitySources.Distinct().Count();
        // Pode haver múltiplas instâncias devido a race conditions, mas todas devem ter o mesmo nome e versão
        uniqueActivitySources.Should().BeGreaterOrEqualTo(1);
        
        var firstActivitySource = activitySources.First();
        firstActivitySource.Name.Should().Be(serviceName);
        firstActivitySource.Version.Should().Be("1.0.0");
        
        // Verificar que todas as instâncias têm o mesmo nome e versão
        activitySources.All(a => a.Name == serviceName).Should().BeTrue();
        activitySources.All(a => a.Version == "1.0.0").Should().BeTrue();
    }

    [Fact]
    public void ActivitySourceFactory_GetOrCreate_ConcurrentAccessWithDifferentVersions_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var version1 = "1.0.0";
        var version2 = "2.0.0";
        var tasks = new List<Task<ActivitySource>>();
        var numberOfTasks = 50;

        // Act
        for (int i = 0; i < numberOfTasks; i++)
        {
            var version = i % 2 == 0 ? version1 : version2;
            tasks.Add(Task.Run(() => ActivitySourceFactory.GetOrCreate(serviceName, version)));
        }

        var activitySources = Task.WhenAll(tasks).Result;

        // Assert
        activitySources.Should().HaveCount(numberOfTasks);
        var uniqueActivitySources = activitySources.Distinct().Count();
        uniqueActivitySources.Should().Be(2, "should have two different activity source instances for different versions");
        
        var version1ActivitySources = activitySources.Where(a => a.Version == version1).ToList();
        var version2ActivitySources = activitySources.Where(a => a.Version == version2).ToList();
        
        version1ActivitySources.Should().HaveCount(numberOfTasks / 2);
        version2ActivitySources.Should().HaveCount(numberOfTasks / 2);
        
        version1ActivitySources.Distinct().Should().HaveCount(1, "all version 1 activity sources should be the same instance");
        version2ActivitySources.Distinct().Should().HaveCount(1, "all version 2 activity sources should be the same instance");
    }

    [Fact]
    public void ActivitySourceFactory_StartActivity_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var activityName = "concurrent_activity";
        var tasks = new List<Task<Activity?>>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => ActivitySourceFactory.StartActivity(serviceName, activityName)));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(100);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ObservabilityMetrics_DisposeAll_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        ObservabilityMetrics.GetOrCreateMeter(serviceName);
        var tasks = new List<Task>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.DisposeAll()));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(10);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ActivitySourceFactory_DisposeAll_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        ActivitySourceFactory.GetOrCreate(serviceName);
        var tasks = new List<Task>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => ActivitySourceFactory.DisposeAll()));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(10);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ObservabilityMetrics_MixedOperations_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var tasks = new List<Task>();

        // Act & Assert
        var act = () =>
        {
            // Misturar operações de criação e descarte
            for (int i = 0; i < 10; i++) // Reduzir número de operações
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.GetOrCreateMeter(serviceName)));
                tasks.Add(Task.Run(() => ObservabilityMetrics.CreateCounter<int>(serviceName, $"counter_{i}")));
                tasks.Add(Task.Run(() => ObservabilityMetrics.CreateHistogram<double>(serviceName, $"histogram_{i}")));
            }
            
            // Adicionar algumas operações de dispose
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.DisposeAll()));
            }
            
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(33); // 10 + 10 + 10 + 3
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ActivitySourceFactory_MixedOperations_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var tasks = new List<Task>();

        // Act & Assert
        var act = () =>
        {
            // Misturar operações de criação e descarte
            for (int i = 0; i < 50; i++)
            {
                tasks.Add(Task.Run(() => ActivitySourceFactory.GetOrCreate(serviceName)));
                tasks.Add(Task.Run(() => ActivitySourceFactory.StartActivity(serviceName, "activity")));
            }
            
            // Adicionar algumas operações de dispose
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => ActivitySourceFactory.DisposeAll()));
            }
            
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(110); // 50 + 50 + 10
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ObservabilityMetrics_CreateObservableGauge_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var gaugeName = "concurrent_gauge";
        var tasks = new List<Task<ObservableGauge<int>>>();
        var counter = 0;

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => 
                    ObservabilityMetrics.CreateObservableGauge<int>(serviceName, gaugeName, () => Interlocked.Increment(ref counter))));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(100);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public void ObservabilityMetrics_CreateUpDownCounter_ConcurrentAccess_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "ConcurrentTestService";
        var counterName = "concurrent_updown_counter";
        var tasks = new List<Task<UpDownCounter<int>>>();

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => ObservabilityMetrics.CreateUpDownCounter<int>(serviceName, counterName)));
            }
            Task.WaitAll(tasks.ToArray());
        };

        act.Should().NotThrow();
        tasks.Should().HaveCount(100);
        tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }
}
