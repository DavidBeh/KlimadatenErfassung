using System.Device.I2c;
using Iot.Device.Ahtxx;
using Iot.Device.Board;

namespace RpiTestBlazor.Services;

public class AhtService
{
    public Aht20 Sensor;

    public AhtService()
    {
        
    }
}