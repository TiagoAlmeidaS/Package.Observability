using FluentAssertions;
using Package.Observability;
using Xunit;

namespace Package.Observability.UnitTests;

public class ConfigurationValidatorTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ShouldReturnValidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            PrometheusPort = 9090,
            EnableMetrics = true,
            EnableTracing = true,
            EnableLogging = true,
            LokiUrl = "http://localhost:3100",
            OtlpEndpoint = "http://localhost:4317",
            MinimumLogLevel = "Information"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyServiceName_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "",
            PrometheusPort = 9090
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("ServiceName não pode ser nulo ou vazio");
    }

    [Fact]
    public void Validate_WithInvalidPrometheusPort_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            PrometheusPort = 70000
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("PrometheusPort deve estar entre 1 e 65535. Valor atual: 70000");
    }

    [Fact]
    public void Validate_WithInvalidLokiUrl_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            LokiUrl = "invalid-url"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("LokiUrl inválida: invalid-url");
    }

    [Fact]
    public void Validate_WithInvalidOtlpEndpoint_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            OtlpEndpoint = "not-a-url"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("OtlpEndpoint inválido: not-a-url");
    }

    [Fact]
    public void Validate_WithInvalidLogLevel_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            MinimumLogLevel = "InvalidLevel"
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("MinimumLogLevel inválido: InvalidLevel"));
    }

    [Fact]
    public void Validate_WithLongServiceName_ShouldReturnWarning()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = new string('A', 60), // 60 caracteres
            PrometheusPort = 9090
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain("ServiceName muito longo (máximo recomendado: 50 caracteres)");
    }

    [Fact]
    public void Validate_WithLowPort_ShouldReturnWarning()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            PrometheusPort = 80
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain("Portas abaixo de 1024 podem requerer privilégios de administrador");
    }

    [Fact]
    public void Validate_WithInvalidAdditionalLabels_ShouldReturnInvalidResult()
    {
        // Arrange
        var options = new ObservabilityOptions
        {
            ServiceName = "TestService",
            AdditionalLabels = new Dictionary<string, string>
            {
                [""] = "value", // Chave vazia
                ["valid"] = "" // Valor vazio
            }
        };

        // Act
        var result = ConfigurationValidator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Chave de label adicional não pode ser nula ou vazia");
        result.Warnings.Should().Contain("Valor do label 'valid' está vazio");
    }

    [Theory]
    [InlineData("http://localhost:3100", true)]
    [InlineData("https://loki.example.com:3100", true)]
    [InlineData("invalid-url", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidUrl_WithVariousInputs_ShouldReturnExpectedResult(string url, bool expected)
    {
        // Act
        var result = ConfigurationValidator.IsValidUrl(url);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1024, true)]
    [InlineData(65535, true)]
    [InlineData(0, false)]
    [InlineData(70000, false)]
    [InlineData(-1, false)]
    public void IsValidPort_WithVariousInputs_ShouldReturnExpectedResult(int port, bool expected)
    {
        // Act
        var result = ConfigurationValidator.IsValidPort(port);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Information", true)]
    [InlineData("DEBUG", true)]
    [InlineData("error", true)]
    [InlineData("InvalidLevel", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidLogLevel_WithVariousInputs_ShouldReturnExpectedResult(string logLevel, bool expected)
    {
        // Act
        var result = ConfigurationValidator.IsValidLogLevel(logLevel);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToString_WithErrorsAndWarnings_ShouldReturnFormattedString()
    {
        // Arrange
        var result = new ValidationResult
        {
            IsValid = false,
            Errors = new List<string> { "Error 1", "Error 2" },
            Warnings = new List<string> { "Warning 1" }
        };

        // Act
        var resultString = result.ToString();

        // Assert
        resultString.Should().Contain("❌ Configuração inválida");
        resultString.Should().Contain("Erros (2):");
        resultString.Should().Contain("• Error 1");
        resultString.Should().Contain("• Error 2");
        resultString.Should().Contain("Avisos (1):");
        resultString.Should().Contain("⚠️ Warning 1");
    }

    [Fact]
    public void ToString_WithValidConfiguration_ShouldReturnValidString()
    {
        // Arrange
        var result = new ValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            Warnings = new List<string>()
        };

        // Act
        var resultString = result.ToString();

        // Assert
        resultString.Should().Contain("✅ Configuração válida");
        resultString.Should().NotContain("Erros");
        resultString.Should().NotContain("Avisos");
    }
}
