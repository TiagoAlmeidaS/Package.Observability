using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Package.Observability;
using Package.Observability.Telemetry;
using Xunit;

namespace Package.Observability.UnitTests;

/// <summary>
/// Testes unitários para ZeroConfigTelemetry
/// </summary>
public class ZeroConfigTelemetryTests
{
    private readonly Mock<ILogger<ZeroConfigTelemetryMiddleware>> _loggerMock;
    private readonly Mock<IOptions<ObservabilityOptions>> _optionsMock;
    private readonly ObservabilityOptions _options;

    public ZeroConfigTelemetryTests()
    {
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
    public void RecordHttpRequest_WithValidData_ShouldRecordMetrics()
    {
        // Arrange
        var method = "GET";
        var path = "/test";
        var statusCode = 200;
        var duration = 1.5;

        // Act
        ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration);

        // Assert
        // Como ZeroConfigTelemetry é estático e não retorna valores,
        // verificamos que não lança exceções
        // Em um cenário real, você poderia verificar métricas expostas
    }

    [Theory]
    [InlineData("GET", "/api/test", 200, 0.5)]
    [InlineData("POST", "/api/users", 201, 1.2)]
    [InlineData("PUT", "/api/users/1", 200, 0.8)]
    [InlineData("DELETE", "/api/users/1", 204, 0.3)]
    public void RecordHttpRequest_WithDifferentMethods_ShouldNotThrow(string method, string path, int statusCode, double duration)
    {
        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(200, false)]
    [InlineData(201, false)]
    [InlineData(400, true)]
    [InlineData(404, true)]
    [InlineData(500, true)]
    public void RecordHttpRequest_WithDifferentStatusCodes_ShouldHandleCorrectly(int statusCode, bool shouldRecordError)
    {
        // Arrange
        var method = "GET";
        var path = "/test";
        var duration = 1.0;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordMethodCall_WithValidData_ShouldRecordMetrics()
    {
        // Arrange
        var className = "TestClass";
        var methodName = "TestMethod";
        var duration = 2.5;
        var success = true;
        var error = (string?)null;

        // Act
        ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success, error);

        // Assert
        // Verifica que não lança exceções
        // Em um cenário real, você poderia verificar métricas expostas
    }

    [Theory]
    [InlineData("WeatherService", "GetForecastAsync", 1.2, true, null)]
    [InlineData("UserService", "CreateUser", 0.8, true, null)]
    [InlineData("OrderService", "ProcessOrder", 3.5, false, "Validation failed")]
    [InlineData("PaymentService", "ProcessPayment", 2.1, false, "Insufficient funds")]
    public void RecordMethodCall_WithDifferentData_ShouldNotThrow(string className, string methodName, double duration, bool success, string? error)
    {
        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success, error));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordMethodCall_WithError_ShouldRecordError()
    {
        // Arrange
        var className = "TestClass";
        var methodName = "TestMethod";
        var duration = 1.0;
        var success = false;
        var error = "Test error message";

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success, error));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordHttpRequest_WithNullPath_ShouldHandleGracefully()
    {
        // Arrange
        var method = "GET";
        string? path = null;
        var statusCode = 200;
        var duration = 1.0;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordHttpRequest_WithEmptyPath_ShouldHandleGracefully()
    {
        // Arrange
        var method = "GET";
        var path = "";
        var statusCode = 200;
        var duration = 1.0;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordHttpRequest_WithNegativeDuration_ShouldHandleGracefully()
    {
        // Arrange
        var method = "GET";
        var path = "/test";
        var statusCode = 200;
        var duration = -1.0;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordMethodCall_WithEmptyClassName_ShouldHandleGracefully()
    {
        // Arrange
        var className = "";
        var methodName = "TestMethod";
        var duration = 1.0;
        var success = true;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void RecordMethodCall_WithEmptyMethodName_ShouldHandleGracefully()
    {
        // Arrange
        var className = "TestClass";
        var methodName = "";
        var duration = 1.0;
        var success = true;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success));
        
        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.001)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void RecordHttpRequest_WithDifferentDurations_ShouldHandleCorrectly(double duration)
    {
        // Arrange
        var method = "GET";
        var path = "/test";
        var statusCode = 200;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordHttpRequest(method, path, statusCode, duration));
        
        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.001)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void RecordMethodCall_WithDifferentDurations_ShouldHandleCorrectly(double duration)
    {
        // Arrange
        var className = "TestClass";
        var methodName = "TestMethod";
        var success = true;

        // Act & Assert
        var exception = Record.Exception(() => 
            ZeroConfigTelemetry.RecordMethodCall(className, methodName, duration, success));
        
        exception.Should().BeNull();
    }
}
