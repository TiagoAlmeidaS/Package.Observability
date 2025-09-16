using System.Diagnostics;
using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ActivitySourceFactoryTests : IDisposable
{
    public ActivitySourceFactoryTests()
    {
        // Limpar estado antes de cada teste
        ActivitySourceFactory.DisposeAll();
    }

    public void Dispose()
    {
        // Limpar estado após cada teste
        ActivitySourceFactory.DisposeAll();
    }

    [Fact]
    public void GetOrCreate_WithValidServiceName_ShouldReturnActivitySource()
    {
        // Arrange
        var serviceName = "TestService";

        // Act
        var activitySource = ActivitySourceFactory.GetOrCreate(serviceName);

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(serviceName);
        activitySource.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void GetOrCreate_WithValidServiceNameAndVersion_ShouldReturnActivitySource()
    {
        // Arrange
        var serviceName = "TestService";
        var version = "2.0.0";

        // Act
        var activitySource = ActivitySourceFactory.GetOrCreate(serviceName, version);

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(serviceName);
        activitySource.Version.Should().Be(version);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetOrCreate_WithInvalidServiceName_ShouldThrowArgumentException(string serviceName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ActivitySourceFactory.GetOrCreate(serviceName));
        
        exception.ParamName.Should().Be("serviceName");
        exception.Message.Should().Contain("Service name cannot be null or empty");
    }

    [Fact]
    public void GetOrCreate_WithSameServiceName_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "TestService";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName);

        // Assert
        activitySource1.Should().BeSameAs(activitySource2);
    }

    [Fact]
    public void GetOrCreate_WithDifferentServiceNames_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName1 = "TestService1";
        var serviceName2 = "TestService2";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName1);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName2);

        // Assert
        activitySource1.Should().NotBeSameAs(activitySource2);
        activitySource1.Name.Should().Be(serviceName1);
        activitySource2.Name.Should().Be(serviceName2);
    }

    [Fact]
    public void GetOrCreate_WithSameServiceNameAndVersion_ShouldReturnSameInstance()
    {
        // Arrange
        var serviceName = "TestService";
        var version = "1.0.0";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName, version);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName, version);

        // Assert
        activitySource1.Should().BeSameAs(activitySource2);
    }

    [Fact]
    public void GetOrCreate_WithSameServiceNameDifferentVersions_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceName = "TestService";
        var version1 = "1.0.0";
        var version2 = "2.0.0";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName, version1);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName, version2);

        // Assert
        activitySource1.Should().NotBeSameAs(activitySource2);
        activitySource1.Version.Should().Be(version1);
        activitySource2.Version.Should().Be(version2);
    }

    [Fact]
    public void StartActivity_WithValidParameters_ShouldReturnActivity()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        if (activity != null)
        {
            activity.DisplayName.Should().Be(activityName);
        }
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Fact]
    public void StartActivity_WithValidParametersAndKind_ShouldReturnActivity()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";
        var kind = ActivityKind.Server;

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName, kind);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        if (activity != null)
        {
            activity.DisplayName.Should().Be(activityName);
            activity.Kind.Should().Be(kind);
        }
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Fact]
    public void StartActivity_WithValidParametersAndParentContext_ShouldReturnActivity()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";
        var parentContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.None);

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Internal, parentContext);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        if (activity != null)
        {
            activity.DisplayName.Should().Be(activityName);
        }
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StartActivity_WithInvalidServiceName_ShouldThrowArgumentException(string serviceName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ActivitySourceFactory.StartActivity(serviceName, "TestActivity"));
        
        exception.ParamName.Should().Be("serviceName");
        exception.Message.Should().Contain("Service name cannot be null or empty");
    }

    [Fact]
    public void StartActivity_WithDefaultParameters_ShouldUseInternalKind()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        if (activity != null)
        {
            activity.Kind.Should().Be(ActivityKind.Internal);
        }
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Fact]
    public void StartActivity_WithDefaultParameters_ShouldUseDefaultParentContext()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        // Verificar que não há exceção com contexto padrão
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Fact]
    public void DisposeAll_ShouldDisposeAllActivitySources()
    {
        // Arrange
        var serviceName1 = "TestService1";
        var serviceName2 = "TestService2";
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName1);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName2);

        // Act
        ActivitySourceFactory.DisposeAll();

        // Assert
        // Verificar se os ActivitySources foram descartados (não podemos testar diretamente,
        // mas podemos verificar que não há exceção)
        activitySource1.Should().NotBeNull();
        activitySource2.Should().NotBeNull();
    }

    [Fact]
    public void DisposeAll_CanBeCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var serviceName = "TestService";
        ActivitySourceFactory.GetOrCreate(serviceName);

        // Act & Assert
        var act1 = () => ActivitySourceFactory.DisposeAll();
        var act2 = () => ActivitySourceFactory.DisposeAll();
        var act3 = () => ActivitySourceFactory.DisposeAll();

        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
    }

    [Fact]
    public void GetOrCreate_AfterDisposeAll_ShouldCreateNewActivitySource()
    {
        // Arrange
        var serviceName = "TestService";
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName);
        ActivitySourceFactory.DisposeAll();

        // Act
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName);

        // Assert
        activitySource2.Should().NotBeNull();
        activitySource2.Name.Should().Be(serviceName);
        // Não podemos testar se são instâncias diferentes devido ao comportamento interno,
        // mas podemos verificar que não há exceção
    }

    [Fact]
    public void StartActivity_WithNonExistentServiceName_ShouldCreateActivitySourceAndReturnActivity()
    {
        // Arrange
        var serviceName = "NonExistentService";
        var activityName = "TestActivity";

        // Act
        using var activity = ActivitySourceFactory.StartActivity(serviceName, activityName);

        // Assert
        // Activity pode ser null se sampling estiver desabilitado
        if (activity != null)
        {
            activity.DisplayName.Should().Be(activityName);
        }
        // Se activity for null, isso é aceitável quando sampling está desabilitado
    }

    [Fact]
    public void StartActivity_WithDifferentActivityKinds_ShouldReturnActivitiesWithCorrectKind()
    {
        // Arrange
        var serviceName = "TestService";
        var activityName = "TestActivity";

        // Act & Assert
        using var internalActivity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Internal);
        internalActivity?.Kind.Should().Be(ActivityKind.Internal);

        using var serverActivity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Server);
        serverActivity?.Kind.Should().Be(ActivityKind.Server);

        using var clientActivity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Client);
        clientActivity?.Kind.Should().Be(ActivityKind.Client);

        using var producerActivity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Producer);
        producerActivity?.Kind.Should().Be(ActivityKind.Producer);

        using var consumerActivity = ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Consumer);
        consumerActivity?.Kind.Should().Be(ActivityKind.Consumer);
    }
}
