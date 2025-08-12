using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Package.Observability;
using Xunit;

namespace Package.Observability.IntegrationTests;

public class EndpointsAndMetricsTests : IClassFixture<ExampleApiFactory>
{
    private readonly ExampleApiFactory _factory;

    public EndpointsAndMetricsTests(ExampleApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GET_WeatherForecast_should_return_200_and_items()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("temperatureC", because: "default weather endpoint returns a list");
    }

    [Fact]
    public async Task GET_Error_should_return_500()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/WeatherForecast/error");
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Prometheus_metrics_endpoint_should_be_exposed_and_contain_runtime_metrics()
    {
        var client = _factory.CreateClient();

        // Hit the API to generate some metrics first
        _ = await client.GetAsync("/WeatherForecast");

        // Fetch metrics via ASP.NET Core endpoint
        var metricsResponse = await client.GetAsync("/metrics");
        metricsResponse.IsSuccessStatusCode.Should().BeTrue();
        var metrics = await metricsResponse.Content.ReadAsStringAsync();

        metrics.Should().Contain("process_runtime_dotnet_gc_heap_size_bytes");
        metrics.Should().Contain("http_server_request_duration_seconds");
        metrics.Should().Contain("weather_requests_total");
    }
}
