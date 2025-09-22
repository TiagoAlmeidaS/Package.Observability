using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Package.Observability.Telemetry;
using Xunit;

namespace Package.Observability.UnitTests;

/// <summary>
/// Testes unitários para ZeroConfigTelemetryMiddleware
/// </summary>
public class ZeroConfigTelemetryMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ZeroConfigTelemetryMiddleware>> _loggerMock;
    private readonly Mock<IOptions<ObservabilityOptions>> _optionsMock;
    private readonly ObservabilityOptions _options;

    public ZeroConfigTelemetryMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ZeroConfigTelemetryMiddleware>>();
        _optionsMock = new Mock<IOptions<ObservabilityOptions>>();
        
        _options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableMetrics = true,
            EnableTracing = true,
            EnableLogging = true
        };
        
        _optionsMock.Setup(o => o.Value).Returns(_options);
    }

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldLogRequestStart()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldLogRequestComplete()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogError()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);
        var exception = new InvalidOperationException("Test exception");

        _nextMock.Setup(n => n(context)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldRethrowException()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);
        var expectedException = new InvalidOperationException("Test exception");

        _nextMock.Setup(n => n(context)).ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
        actualException.Should().Be(expectedException);
    }

    [Theory]
    [InlineData("GET", "/api/test")]
    [InlineData("POST", "/api/users")]
    [InlineData("PUT", "/api/users/1")]
    [InlineData("DELETE", "/api/users/1")]
    public async Task InvokeAsync_WithDifferentMethods_ShouldHandleCorrectly(string method, string path)
    {
        // Arrange
        var context = CreateHttpContext(method, path);
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/api/test")]
    [InlineData("/api/users/123")]
    [InlineData("/health")]
    [InlineData("/metrics")]
    public async Task InvokeAsync_WithDifferentPaths_ShouldHandleCorrectly(string path)
    {
        // Arrange
        var context = CreateHttpContext("GET", path);
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithNullPath_ShouldHandleGracefully()
    {
        // Arrange
        var context = CreateHttpContext("GET", null);
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithObservabilityDisabled_ShouldSkipProcessing()
    {
        // Arrange
        var disabledOptions = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableMetrics = false,
            EnableTracing = false,
            EnableLogging = false
        };
        
        var disabledOptionsMock = new Mock<IOptions<ObservabilityOptions>>();
        disabledOptionsMock.Setup(o => o.Value).Returns(disabledOptions);

        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, disabledOptionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
        // Não deve logar nada quando observabilidade está desabilitada
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithMetricsDisabled_ShouldSkipMetrics()
    {
        // Arrange
        var metricsDisabledOptions = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableMetrics = false,
            EnableTracing = true,
            EnableLogging = true
        };
        
        var metricsDisabledOptionsMock = new Mock<IOptions<ObservabilityOptions>>();
        metricsDisabledOptionsMock.Setup(o => o.Value).Returns(metricsDisabledOptions);

        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, metricsDisabledOptionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithLoggingDisabled_ShouldSkipLogging()
    {
        // Arrange
        var loggingDisabledOptions = new ObservabilityOptions
        {
            ServiceName = "TestService",
            EnableMetrics = true,
            EnableTracing = true,
            EnableLogging = false
        };
        
        var loggingDisabledOptionsMock = new Mock<IOptions<ObservabilityOptions>>();
        loggingDisabledOptionsMock.Setup(o => o.Value).Returns(loggingDisabledOptions);

        var context = CreateHttpContext("GET", "/test");
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, loggingDisabledOptionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithDifferentStatusCode_ShouldHandleCorrectly()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        context.Response.StatusCode = 404;
        
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithRemoteIpAddress_ShouldLogCorrectly()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/test");
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        
        var middleware = new ZeroConfigTelemetryMiddleware(_nextMock.Object, _loggerMock.Object, _optionsMock.Object);

        _nextMock.Setup(n => n(context)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private HttpContext CreateHttpContext(string method, string? path)
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var connection = new Mock<ConnectionInfo>();

        request.Setup(r => r.Method).Returns(method);
        request.Setup(r => r.Path).Returns(new PathString(path ?? "/"));
        response.Setup(r => r.StatusCode).Returns(200);
        connection.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Loopback);

        context.Setup(c => c.Request).Returns(request.Object);
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Connection).Returns(connection.Object);

        return context.Object;
    }

}
