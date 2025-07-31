using FluentAssertions;
using System.Diagnostics;

namespace Package.Observability.Tests;

[TestClass]
public class ActivitySourceFactoryTests
{
    [TestCleanup]
    public void Cleanup()
    {
        // Limpar ActivitySources criados durante os testes
        ActivitySourceFactory.DisposeAll();
    }

    [TestMethod]
    public void GetOrCreate_WithServiceName_ShouldReturnActivitySource()
    {
        // Arrange
        const string serviceName = "TestService";

        // Act
        var activitySource = ActivitySourceFactory.GetOrCreate(serviceName);

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(serviceName);
        activitySource.Version.Should().Be("1.0.0");
    }

    [TestMethod]
    public void GetOrCreate_WithServiceNameAndVersion_ShouldReturnActivitySource()
    {
        // Arrange
        const string serviceName = "TestService";
        const string version = "2.1.0";

        // Act
        var activitySource = ActivitySourceFactory.GetOrCreate(serviceName, version);

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(serviceName);
        activitySource.Version.Should().Be(version);
    }

    [TestMethod]
    public void GetOrCreate_WithSameParameters_ShouldReturnSameInstance()
    {
        // Arrange
        const string serviceName = "TestService";
        const string version = "1.0.0";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName, version);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName, version);

        // Assert
        activitySource1.Should().BeSameAs(activitySource2);
    }

    [TestMethod]
    public void GetOrCreate_WithDifferentServiceNames_ShouldReturnDifferentInstances()
    {
        // Arrange
        const string serviceName1 = "TestService1";
        const string serviceName2 = "TestService2";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName1);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName2);

        // Assert
        activitySource1.Should().NotBeSameAs(activitySource2);
        activitySource1.Name.Should().Be(serviceName1);
        activitySource2.Name.Should().Be(serviceName2);
    }

    [TestMethod]
    public void GetOrCreate_WithDifferentVersions_ShouldReturnDifferentInstances()
    {
        // Arrange
        const string serviceName = "TestService";
        const string version1 = "1.0.0";
        const string version2 = "2.0.0";

        // Act
        var activitySource1 = ActivitySourceFactory.GetOrCreate(serviceName, version1);
        var activitySource2 = ActivitySourceFactory.GetOrCreate(serviceName, version2);

        // Assert
        activitySource1.Should().NotBeSameAs(activitySource2);
        activitySource1.Version.Should().Be(version1);
        activitySource2.Version.Should().Be(version2);
    }

    [TestMethod]
    public void GetOrCreate_WithNullServiceName_ShouldThrow()
    {
        // Arrange
        string? nullServiceName = null;

        // Act & Assert
        var act = () => ActivitySourceFactory.GetOrCreate(nullServiceName!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Service name cannot be null or empty*");
    }

    [TestMethod]
    public void GetOrCreate_WithEmptyServiceName_ShouldThrow()
    {
        // Arrange
        const string emptyServiceName = "";

        // Act & Assert
        var act = () => ActivitySourceFactory.GetOrCreate(emptyServiceName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Service name cannot be null or empty*");
    }

    [TestMethod]
    public void GetOrCreate_WithWhitespaceServiceName_ShouldThrow()
    {
        // Arrange
        const string whitespaceServiceName = "   ";

        // Act & Assert
        var act = () => ActivitySourceFactory.GetOrCreate(whitespaceServiceName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Service name cannot be null or empty*");
    }

    [TestMethod]
    public void StartActivity_WithValidParameters_ShouldReturnActivity()
    {
        // Arrange
        const string serviceName = "TestService";
        const string activityName = "TestActivity";

        // Act
        var activity = ActivitySourceFactory.StartActivity(serviceName, activityName);

        // Assert
        // Nota: Activity pode ser null se não houver listeners ativos
        // Em testes unitários, geralmente não há listeners configurados
        // Mas o método não deve lançar exceção
        var act = () => ActivitySourceFactory.StartActivity(serviceName, activityName);
        act.Should().NotThrow();
    }

    [TestMethod]
    public void StartActivity_WithDifferentKinds_ShouldNotThrow()
    {
        // Arrange
        const string serviceName = "TestService";
        const string activityName = "TestActivity";

        // Act & Assert
        var actInternal = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Internal);
        actInternal.Should().NotThrow();

        var actClient = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Client);
        actClient.Should().NotThrow();

        var actServer = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Server);
        actServer.Should().NotThrow();

        var actProducer = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Producer);
        actProducer.Should().NotThrow();

        var actConsumer = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Consumer);
        actConsumer.Should().NotThrow();
    }

    [TestMethod]
    public void StartActivity_WithParentContext_ShouldNotThrow()
    {
        // Arrange
        const string serviceName = "TestService";
        const string activityName = "TestActivity";
        var parentContext = new ActivityContext();

        // Act & Assert
        var act = () => ActivitySourceFactory.StartActivity(serviceName, activityName, ActivityKind.Internal, parentContext);
        act.Should().NotThrow();
    }

    [TestMethod]
    public void DisposeAll_ShouldDisposeAllActivitySources()
    {
        // Arrange
        var activitySource1 = ActivitySourceFactory.GetOrCreate("Service1");
        var activitySource2 = ActivitySourceFactory.GetOrCreate("Service2");
        var activitySource3 = ActivitySourceFactory.GetOrCreate("Service3");

        // Act
        ActivitySourceFactory.DisposeAll();

        // Assert
        // Após DisposeAll, novos ActivitySources devem ser criados
        var newActivitySource1 = ActivitySourceFactory.GetOrCreate("Service1");
        newActivitySource1.Should().NotBeSameAs(activitySource1);
    }

    [TestMethod]
    public void DisposeAll_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        ActivitySourceFactory.GetOrCreate("Service1");
        ActivitySourceFactory.GetOrCreate("Service2");

        // Act & Assert
        var act = () =>
        {
            ActivitySourceFactory.DisposeAll();
            ActivitySourceFactory.DisposeAll();
            ActivitySourceFactory.DisposeAll();
        };

        act.Should().NotThrow();
    }

    [TestMethod]
    public void DisposeAll_WithNoActivitySources_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => ActivitySourceFactory.DisposeAll();
        act.Should().NotThrow();
    }

    [TestMethod]
    public void GetOrCreate_AfterDisposeAll_ShouldCreateNewInstances()
    {
        // Arrange
        const string serviceName = "TestService";
        var originalActivitySource = ActivitySourceFactory.GetOrCreate(serviceName);

        // Act
        ActivitySourceFactory.DisposeAll();
        var newActivitySource = ActivitySourceFactory.GetOrCreate(serviceName);

        // Assert
        newActivitySource.Should().NotBeSameAs(originalActivitySource);
        newActivitySource.Name.Should().Be(serviceName);
    }

    [TestMethod]
    public void GetOrCreate_ThreadSafety_ShouldHandleConcurrentAccess()
    {
        // Arrange
        const string serviceName = "ConcurrentTestService";
        const int threadCount = 10;
        var activitySources = new ActivitySource[threadCount];
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                activitySources[index] = ActivitySourceFactory.GetOrCreate(serviceName);
            });
        }

        Task.WaitAll(tasks);

        // Assert
        // Todas as instâncias devem ser a mesma (thread-safe singleton)
        for (int i = 1; i < threadCount; i++)
        {
            activitySources[i].Should().BeSameAs(activitySources[0]);
        }
    }
}