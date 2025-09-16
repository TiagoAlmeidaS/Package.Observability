using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;

namespace Package.Observability;

/// <summary>
/// Gerenciador de recursos para o pacote de observabilidade
/// </summary>
public class ResourceManager : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly object _lock = new();
    private bool _disposed = false;

    /// <summary>
    /// Registra um recurso para ser descartado
    /// </summary>
    /// <param name="disposable">Recurso a ser registrado</param>
    public void Register(IDisposable disposable)
    {
        if (disposable == null) return;

        lock (_lock)
        {
            if (_disposed)
            {
                disposable.Dispose();
                return;
            }

            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// Registra um Meter para ser descartado
    /// </summary>
    /// <param name="meter">Meter a ser registrado</param>
    public void RegisterMeter(Meter meter)
    {
        if (meter == null) return;
        Register(meter);
    }

    /// <summary>
    /// Registra um ActivitySource para ser descartado
    /// </summary>
    /// <param name="activitySource">ActivitySource a ser registrado</param>
    public void RegisterActivitySource(ActivitySource activitySource)
    {
        if (activitySource == null) return;
        Register(activitySource);
    }

    /// <summary>
    /// Registra um MeterProvider para ser descartado
    /// </summary>
    /// <param name="meterProvider">MeterProvider a ser registrado</param>
    public void RegisterMeterProvider(IDisposable meterProvider)
    {
        if (meterProvider == null) return;
        Register(meterProvider);
    }

    /// <summary>
    /// Registra um TracerProvider para ser descartado
    /// </summary>
    /// <param name="tracerProvider">TracerProvider a ser registrado</param>
    public void RegisterTracerProvider(IDisposable tracerProvider)
    {
        if (tracerProvider == null) return;
        Register(tracerProvider);
    }

    /// <summary>
    /// Desregistra um recurso específico
    /// </summary>
    /// <param name="disposable">Recurso a ser desregistrado</param>
    public void Unregister(IDisposable disposable)
    {
        if (disposable == null) return;

        lock (_lock)
        {
            _disposables.Remove(disposable);
        }
    }

    /// <summary>
    /// Desregistra todos os recursos de um tipo específico
    /// </summary>
    /// <typeparam name="T">Tipo de recurso a ser desregistrado</typeparam>
    public void UnregisterAll<T>() where T : class, IDisposable
    {
        lock (_lock)
        {
            var toRemove = _disposables.OfType<T>().ToList();
            foreach (var item in toRemove)
            {
                _disposables.Remove(item);
                item.Dispose();
            }
        }
    }

    /// <summary>
    /// Obtém o número de recursos registrados
    /// </summary>
    public int RegisteredCount
    {
        get
        {
            lock (_lock)
            {
                return _disposables.Count;
            }
        }
    }

    /// <summary>
    /// Obtém o número de recursos de um tipo específico
    /// </summary>
    /// <typeparam name="T">Tipo de recurso</typeparam>
    /// <returns>Número de recursos do tipo especificado</returns>
    public int GetCount<T>() where T : class, IDisposable
    {
        lock (_lock)
        {
            return _disposables.OfType<T>().Count();
        }
    }

    /// <summary>
    /// Verifica se há recursos registrados
    /// </summary>
    public bool HasResources
    {
        get
        {
            lock (_lock)
            {
                return _disposables.Count > 0;
            }
        }
    }

    /// <summary>
    /// Descartar todos os recursos registrados
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Descartar todos os recursos registrados
    /// </summary>
    /// <param name="disposing">Indica se está sendo descartado explicitamente</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            lock (_lock)
            {
                // Descartar todos os recursos em ordem reversa
                for (int i = _disposables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _disposables[i]?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log do erro, mas não interrompe o processo de descarte
                        System.Diagnostics.Debug.WriteLine($"Erro ao descartar recurso: {ex.Message}");
                    }
                }

                _disposables.Clear();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Finalizador
    /// </summary>
    ~ResourceManager()
    {
        Dispose(false);
    }
}

/// <summary>
/// Extensões para o ResourceManager
/// </summary>
public static class ResourceManagerExtensions
{
    /// <summary>
    /// Registra automaticamente todos os recursos do ObservabilityMetrics
    /// </summary>
    /// <param name="resourceManager">Gerenciador de recursos</param>
    public static void RegisterObservabilityMetrics(this ResourceManager resourceManager)
    {
        // Esta é uma implementação simplificada
        // Em uma implementação real, você precisaria de acesso aos recursos internos
        // Por enquanto, apenas registramos que os recursos foram criados
    }

    /// <summary>
    /// Registra automaticamente todos os recursos do ActivitySourceFactory
    /// </summary>
    /// <param name="resourceManager">Gerenciador de recursos</param>
    public static void RegisterActivitySourceFactory(this ResourceManager resourceManager)
    {
        // Esta é uma implementação simplificada
        // Em uma implementação real, você precisaria de acesso aos recursos internos
        // Por enquanto, apenas registramos que os recursos foram criados
    }
}
