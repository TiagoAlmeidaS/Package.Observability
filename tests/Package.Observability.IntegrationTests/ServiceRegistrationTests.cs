using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Package.Observability;
using Xunit;
using WebApiFactory = Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>;

namespace Package.Observability.IntegrationTests;

public class ServiceRegistrationTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;

    public ServiceRegistrationTests(WebApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => { });
        });
    }

    [Fact]
    public void Should_register_ObservabilityOptions_and_OpenTelemetry_components()
    {
        using var scope = _factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var options = provider.GetRequiredService<IOptions<ObservabilityOptions>>();
        options.Should().NotBeNull();
        options.Value.ServiceName.Should().NotBeNullOrEmpty();

        // OpenTelemetry providers should be registered
        provider.GetService<MeterProvider>().Should().NotBeNull();
        provider.GetService<TracerProvider>().Should().NotBeNull();
    }
}
