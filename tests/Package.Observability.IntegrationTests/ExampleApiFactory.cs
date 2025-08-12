using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class ExampleApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:EnableTracing"] = "false",
                    ["Observability:EnableLogging"] = "false",
                    ["Observability:EnableAspNetCoreInstrumentation"] = "true",
                    ["Observability:EnableHttpClientInstrumentation"] = "false",
                    ["Observability:EnableRuntimeInstrumentation"] = "true",
                    ["Observability:ServiceName"] = "WebApi.Example",
                    ["Observability:OtlpEndpoint"] = "http://127.0.0.1:4317",
                    ["Observability:PrometheusPort"] = GetFreeTcpPort().ToString(),
                    // Keep defaults for labels
                })
                .Build();

            configBuilder.AddConfiguration(config);
        });
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
