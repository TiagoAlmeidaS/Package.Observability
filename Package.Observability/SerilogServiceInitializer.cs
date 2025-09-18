using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Package.Observability;

/// <summary>
/// Hosted service to initialize SerilogService after DI container is built
/// </summary>
public class SerilogServiceInitializer : IHostedService
{
    private readonly ISerilogService _serilogService;
    private readonly ILogger<SerilogServiceInitializer> _logger;

    public SerilogServiceInitializer(ISerilogService serilogService, ILogger<SerilogServiceInitializer> logger)
    {
        _serilogService = serilogService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Inicializando SerilogService...");
            
            var success = _serilogService.Configure();
            if (success)
            {
                _logger.LogInformation("SerilogService configurado com sucesso");
            }
            else
            {
                _logger.LogWarning("Falha ao configurar SerilogService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar SerilogService");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Finalizando SerilogService...");
            _serilogService.Flush();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar SerilogService");
        }

        return Task.CompletedTask;
    }
}
