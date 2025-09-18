using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class SerilogServiceInitializerTests
{
    private readonly Mock<ISerilogService> _mockSerilogService;
    private readonly Mock<ILogger<SerilogServiceInitializer>> _mockLogger;

    public SerilogServiceInitializerTests()
    {
        _mockSerilogService = new Mock<ISerilogService>();
        _mockLogger = new Mock<ILogger<SerilogServiceInitializer>>();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Assert
        initializer.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_WhenConfigureReturnsTrue_ShouldLogSuccess()
    {
        // Arrange
        _mockSerilogService.Setup(x => x.Configure()).Returns(true);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Assert
        _mockSerilogService.Verify(x => x.Configure(), Times.AtLeast(1));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SerilogService configurado com sucesso")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenConfigureReturnsFalse_ShouldLogWarning()
    {
        // Arrange
        _mockSerilogService.Setup(x => x.Configure()).Returns(false);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Assert
        _mockSerilogService.Verify(x => x.Configure(), Times.AtLeast(1));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Falha ao configurar SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenConfigureThrowsException_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _mockSerilogService.Setup(x => x.Configure()).Throws(exception);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Assert
        _mockSerilogService.Verify(x => x.Configure(), Times.AtLeast(1));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao inicializar SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenCancellationTokenIsCancelled_ShouldNotThrow()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act & Assert
        var action = async () => await initializer.StartAsync(cancellationTokenSource.Token);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_WhenFlushSucceeds_ShouldLogSuccess()
    {
        // Arrange
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StopAsync(CancellationToken.None);

        // Assert
        _mockSerilogService.Verify(x => x.Flush(), Times.AtLeast(1));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Finalizando SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenFlushThrowsException_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _mockSerilogService.Setup(x => x.Flush()).Throws(exception);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StopAsync(CancellationToken.None);

        // Assert
        _mockSerilogService.Verify(x => x.Flush(), Times.AtLeast(1));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao finalizar SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenCancellationTokenIsCancelled_ShouldNotThrow()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act & Assert
        var action = async () => await initializer.StopAsync(cancellationTokenSource.Token);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_ShouldCallConfigureOnlyOnce()
    {
        // Arrange
        _mockSerilogService.Setup(x => x.Configure()).Returns(true);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);
        await initializer.StartAsync(CancellationToken.None); // Call again

        // Assert
        _mockSerilogService.Verify(x => x.Configure(), Times.AtLeast(1));
    }

    [Fact]
    public async Task StopAsync_ShouldCallFlushOnlyOnce()
    {
        // Arrange
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StopAsync(CancellationToken.None);
        await initializer.StopAsync(CancellationToken.None); // Call again

        // Assert
        _mockSerilogService.Verify(x => x.Flush(), Times.AtLeast(1));
    }

    [Fact]
    public async Task StartAsync_ShouldLogInitializationMessage()
    {
        // Arrange
        _mockSerilogService.Setup(x => x.Configure()).Returns(true);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inicializando SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldLogFinalizationMessage()
    {
        // Arrange
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        await initializer.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Finalizando SerilogService")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithMultipleCalls_ShouldHandleGracefully()
    {
        // Arrange
        _mockSerilogService.Setup(x => x.Configure()).Returns(true);
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        var task1 = initializer.StartAsync(CancellationToken.None);
        var task2 = initializer.StartAsync(CancellationToken.None);
        await Task.WhenAll(task1, task2);

        // Assert
        _mockSerilogService.Verify(x => x.Configure(), Times.AtLeast(1));
    }

    [Fact]
    public async Task StopAsync_WithMultipleCalls_ShouldHandleGracefully()
    {
        // Arrange
        var initializer = new SerilogServiceInitializer(_mockSerilogService.Object, _mockLogger.Object);

        // Act
        var task1 = initializer.StopAsync(CancellationToken.None);
        var task2 = initializer.StopAsync(CancellationToken.None);
        await Task.WhenAll(task1, task2);

        // Assert
        _mockSerilogService.Verify(x => x.Flush(), Times.AtLeast(1));
    }
}
