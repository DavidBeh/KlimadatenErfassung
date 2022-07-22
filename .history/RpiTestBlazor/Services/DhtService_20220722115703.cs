using System.Device.Gpio;
using Iot.Device.DHTxx;
using UnitsNet;

namespace RpiTestBlazor.Services;

public class DhtService
{
    public List<double> Temps;

    public DhtService(ILogger<DhtService> l)
    {
        var d = new Dht11(11, PinNumberingScheme.Board);
        Task.Run(action: async () =>
        {
            while (true)
            {
            
            
                var success = d.TryReadHumidity(out var humidity);
                l.LogInformation(success ? $"{humidity.Percent}" : "Error");
                await Task.Delay(5000);
            }


        });
    }
}