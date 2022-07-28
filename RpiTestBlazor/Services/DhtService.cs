using System.Device.Gpio;
using Iot.Device.DHTxx;
using Iot.Device.OneWire;
using UnitsNet;

namespace RpiTestBlazor.Services;

public class DhtService : IHostedService
{
    public List<double> Temps;
    public Dht11 sensor;
    public DhtService(ILogger<DhtService> l)
    {
        sensor = new Dht11(11, PinNumberingScheme.Board);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(action:async () =>
        {
            
            while (true)
            {
                await Task.Delay(5000);
                var successTemp = sensor.TryReadTemperature(out var temperature);
                var successHum = sensor.TryReadHumidity(out var humidity);
                
                
            }
        });
        return Task.CompletedTask;
    }
    
    
    public class DhtRecord
    {
        public double? Temperature;
        public double? Humidity;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}