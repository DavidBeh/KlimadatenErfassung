namespace GN.Klima.IoT.Services;

public class SensorManager : IHostedService
{

    private readonly ConfigurationService _configuration;
    private readonly IServiceProvider _services;

    public SensorManagerOptions Options => _currentOptions with { };
    private SensorManagerOptions _currentOptions;
    public SensorManager(ConfigurationService configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
    }
    
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        
        _configuration.Load("config.json", stream => );
    }
    
    public void SaveConfiguration()

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
        throw new NotImplementedException();
    }

    public record SensorManagerOptions
    {
        
        public SensorManagerOptions Copy()
        {
            return this with { };
        }
    }
}