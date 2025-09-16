using System.Diagnostics;
using System.Diagnostics.Metrics;
using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ResourceManagerTests : IDisposable
{
    private readonly ResourceManager _resourceManager;

    public ResourceManagerTests()
    {
        _resourceManager = new ResourceManager();
    }

    public void Dispose()
    {
        _resourceManager?.Dispose();
    }

    [Fact]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        resourceManager.RegisteredCount.Should().Be(0);
        resourceManager.HasResources.Should().BeFalse();
    }

    [Fact]
    public void Register_WithValidDisposable_ShouldAddToCollection()
    {
        // Arrange
        var disposable = new TestDisposable();

        // Act
        _resourceManager.Register(disposable);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(1);
        _resourceManager.HasResources.Should().BeTrue();
    }

    [Fact]
    public void Register_WithNullDisposable_ShouldNotAddToCollection()
    {
        // Act
        _resourceManager.Register(null);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(0);
        _resourceManager.HasResources.Should().BeFalse();
    }

    [Fact]
    public void Register_WithMultipleDisposables_ShouldAddAllToCollection()
    {
        // Arrange
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        var disposable3 = new TestDisposable();

        // Act
        _resourceManager.Register(disposable1);
        _resourceManager.Register(disposable2);
        _resourceManager.Register(disposable3);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(3);
        _resourceManager.HasResources.Should().BeTrue();
    }

    [Fact]
    public void RegisterMeter_WithValidMeter_ShouldAddToCollection()
    {
        // Arrange
        var meter = new Meter("TestMeter");

        // Act
        _resourceManager.RegisterMeter(meter);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(1);
        _resourceManager.GetCount<Meter>().Should().Be(1);
    }

    [Fact]
    public void RegisterMeter_WithNullMeter_ShouldNotAddToCollection()
    {
        // Act
        _resourceManager.RegisterMeter(null);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(0);
        _resourceManager.GetCount<Meter>().Should().Be(0);
    }

    [Fact]
    public void RegisterActivitySource_WithValidActivitySource_ShouldAddToCollection()
    {
        // Arrange
        var activitySource = new ActivitySource("TestActivitySource");

        // Act
        _resourceManager.RegisterActivitySource(activitySource);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(1);
        _resourceManager.GetCount<ActivitySource>().Should().Be(1);
    }

    [Fact]
    public void RegisterActivitySource_WithNullActivitySource_ShouldNotAddToCollection()
    {
        // Act
        _resourceManager.RegisterActivitySource(null);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(0);
        _resourceManager.GetCount<ActivitySource>().Should().Be(0);
    }

    [Fact]
    public void Unregister_WithExistingDisposable_ShouldRemoveFromCollection()
    {
        // Arrange
        var disposable = new TestDisposable();
        _resourceManager.Register(disposable);

        // Act
        _resourceManager.Unregister(disposable);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(0);
        _resourceManager.HasResources.Should().BeFalse();
    }

    [Fact]
    public void Unregister_WithNonExistingDisposable_ShouldNotChangeCollection()
    {
        // Arrange
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        _resourceManager.Register(disposable1);

        // Act
        _resourceManager.Unregister(disposable2);

        // Assert
        _resourceManager.RegisteredCount.Should().Be(1);
        _resourceManager.HasResources.Should().BeTrue();
    }

    [Fact]
    public void UnregisterAll_WithSpecificType_ShouldRemoveOnlyThatType()
    {
        // Arrange
        var meter = new Meter("TestMeter");
        var activitySource = new ActivitySource("TestActivitySource");
        var disposable = new TestDisposable();

        _resourceManager.RegisterMeter(meter);
        _resourceManager.RegisterActivitySource(activitySource);
        _resourceManager.Register(disposable);

        // Act
        _resourceManager.UnregisterAll<Meter>();

        // Assert
        _resourceManager.RegisteredCount.Should().Be(2);
        _resourceManager.GetCount<Meter>().Should().Be(0);
        _resourceManager.GetCount<ActivitySource>().Should().Be(1);
        _resourceManager.GetCount<TestDisposable>().Should().Be(1);
    }

    [Fact]
    public void Dispose_ShouldDisposeAllRegisteredResources()
    {
        // Arrange
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        var disposable3 = new TestDisposable();

        _resourceManager.Register(disposable1);
        _resourceManager.Register(disposable2);
        _resourceManager.Register(disposable3);

        // Act
        _resourceManager.Dispose();

        // Assert
        disposable1.IsDisposed.Should().BeTrue();
        disposable2.IsDisposed.Should().BeTrue();
        disposable3.IsDisposed.Should().BeTrue();
        _resourceManager.RegisteredCount.Should().Be(0);
        _resourceManager.HasResources.Should().BeFalse();
    }

    [Fact]
    public void Dispose_WithDisposalError_ShouldContinueDisposingOthers()
    {
        // Arrange
        var disposable1 = new TestDisposable();
        var disposable2 = new FailingDisposable();
        var disposable3 = new TestDisposable();

        _resourceManager.Register(disposable1);
        _resourceManager.Register(disposable2);
        _resourceManager.Register(disposable3);

        // Act
        _resourceManager.Dispose();

        // Assert
        disposable1.IsDisposed.Should().BeTrue();
        disposable3.IsDisposed.Should().BeTrue();
        _resourceManager.RegisteredCount.Should().Be(0);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var disposable = new TestDisposable();
        _resourceManager.Register(disposable);

        // Act & Assert
        _resourceManager.Dispose();
        _resourceManager.Dispose();
        _resourceManager.Dispose();
    }

    [Fact]
    public void Register_AfterDispose_ShouldDisposeImmediately()
    {
        // Arrange
        _resourceManager.Dispose();
        var disposable = new TestDisposable();

        // Act
        _resourceManager.Register(disposable);

        // Assert
        disposable.IsDisposed.Should().BeTrue();
        _resourceManager.RegisteredCount.Should().Be(0);
    }

    [Fact]
    public void GetCount_WithNoResources_ShouldReturnZero()
    {
        // Act
        var count = _resourceManager.GetCount<TestDisposable>();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void GetCount_WithMixedResources_ShouldReturnCorrectCount()
    {
        // Arrange
        var meter1 = new Meter("TestMeter1");
        var meter2 = new Meter("TestMeter2");
        var activitySource = new ActivitySource("TestActivitySource");
        var disposable = new TestDisposable();

        _resourceManager.RegisterMeter(meter1);
        _resourceManager.RegisterMeter(meter2);
        _resourceManager.RegisterActivitySource(activitySource);
        _resourceManager.Register(disposable);

        // Act & Assert
        _resourceManager.GetCount<Meter>().Should().Be(2);
        _resourceManager.GetCount<ActivitySource>().Should().Be(1);
        _resourceManager.GetCount<TestDisposable>().Should().Be(1);
        _resourceManager.RegisteredCount.Should().Be(4);
    }
}

public class TestDisposable : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

public class FailingDisposable : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        throw new InvalidOperationException("Simulated disposal error");
    }
}
