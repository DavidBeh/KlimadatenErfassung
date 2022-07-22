using System.Device.Gpio;
using Iot.Device.DHTxx;
using UnitsNet;

namespace RpiTestBlazor.Services;

public class DhtService : IHostedService
{
    public List<double> Temps;
    public Dht11 d;
    public DhtService(ILogger<DhtService> l)
    {
        d = new Dht11(11, PinNumberingScheme.Board);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(action:async () =>
        {
            
            while (true)
            {
                await Task.Delay(5000);
                var successTemp = d.TryReadTemperature(out var temperature);
                var successHum = d.TryReadHumidity(out var humidity);
                
                
            }
        });
        return Task.CompletedTask;
    }

    public event DhtRecord record;
    
    public class DhtRecord
    {
        public double? Temperature;
        public double? Humidity;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}