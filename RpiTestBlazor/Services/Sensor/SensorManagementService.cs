using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Device.Gpio;
using System.Device.I2c;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iot.Device.Ahtxx;
using Iot.Device.Board;
using UnitsNet;

namespace RpiTestBlazor.Services.Sensor;

class SensorManagementService
{
    private List<SensorEntry> Sensors;

    
    
    public SensorManagementService()
    {
    }

    record SensorEntry
    {
        
        public Guid Id { get; init; } = Guid.NewGuid();
        public int Interval { get; set; } = 5;
        public BaseSensorConfig SensorConfig { get; set; } = null!;
    }


    /*
    private class SensorConfigConverter : JsonConverter<BaseSensorConfig>
    {
        private ConcurrentDictionary<Guid, Type> _types = new ConcurrentDictionary<Guid, Type>();

        private Type? GetType(Guid g)
        {
            if (!_types.TryGetValue(g, out var t))
            {
                foreach (var type1 in AppDomain.CurrentDomain.GetAssemblies()
                             .SelectMany(assembly => assembly.GetTypes())
                             .Where(type => type.IsSubclassOf(typeof(BaseSensorConfig))))
                {
                    _types.TryAdd(type1.GUID, type1);
                }

                _types.TryGetValue(g, out t);

                return t;
            }
            return t;
        }
        
        public override BaseSensorConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var clone = reader;
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
            BaseSensorConfig? b;
            Type t;
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName when reader.GetString() == "§Guid":
                        
                        break;
                    case JsonTokenType.StartObject: reader.Skip();
                        break;
                    case JsonTokenType.EndObject: throw new JsonException();
                    
                }
            }

            return b;
        }

        public override void Write(Utf8JsonWriter writer, BaseSensorConfig value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    */
}

[Guid("0AABA153-E8ED-4A7C-AF1D-6D7C87D81FC6")]
abstract class SensorInstance<T> where T : BaseSensorConfig
{
    
}



interface ITemperature
{
    Temperature GetTemperature();
}

internal interface IHumidity
{
    RelativeHumidity GetHumidity();
}

internal interface IPressure
{
    Pressure GetPressure();
}


abstract record BaseSensorConfig
{
    
}

[Guid("8854A86A-F1E2-4A69-8EAC-0D7C2BFE772B")]
record Aht20SensorConfig : BaseSensorConfig
{
    [DisplayName("Bus Id")]
    public int BusId { get; set; } = 1;
}



class Aht20SensorInstance : SensorInstance<Aht20SensorConfig>, IHumidity, ITemperature
{
    public RelativeHumidity GetHumidity()
    {
        throw new NotImplementedException();
    }

    public Temperature GetTemperature()
    {
        throw new NotImplementedException();
    }
}