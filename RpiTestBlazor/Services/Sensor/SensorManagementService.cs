using System.ComponentModel;
using System.Device.I2c;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Iot.Device.Ahtxx;
using Timer = System.Timers.Timer;
using UnitsNet;

namespace RpiTestBlazor.Services.Sensor;

class SensorManagementService
{
    public enum State
    {
        Initializing,
        Pending,
        Starting,
        Running,
        Stopping,
        Stopped
    }


    private readonly SemaphoreSlim _lock = new(1);

    private CancellationToken _sensorStop;
    public Dictionary<SensorModel, SensorInstance?> CurrentSensors = new();
    public List<SensorModel>? PendingSensors = null;

    public SensorManagementService()
    {
        SensorConfigurationTypes.Add(typeof(Aht20SensorConnection));
    }

    public ICollection<Type> SensorConfigurationTypes { get; } = new List<Type>();

    public event EventHandler<SensorStateChangedEventArgs> SensorStateChanged = delegate { };
    public event EventHandler<SensorMeasuredEventArgs> SensorMeasured = delegate { };

    public event EventHandler SensorsUpdated = delegate { };

    public SensorModel GenerateSensorEntry(Type t)
    {
        var n = new SensorModel()
        {
            SensorConnection = (SensorConnection)Activator.CreateInstance(t)!
        };
        return n;
    }


    public async Task UpdateConfiguration(List<SensorModel> sensorModels)
    {
        PendingSensors = sensorModels.Select(model => model.DeepCopy()).ToList();
        foreach (var (s, i) in CurrentSensors)
        {
            for (int j = 0; j < 2; j++)
            {
                await Task.Run(() => i?.Stop());
                if (i?.CurrentState == State.Stopped)
                    break;
            }
        }

        CurrentSensors.Clear();
        
        foreach (var model in PendingSensors)
        {
            var instance = new SensorInstance(model);
            CurrentSensors.Add(model, instance);
        }
    }

    public async Task StopAsync()
    {
        foreach (var (sensorConfigurationModel, inst) in CurrentSensors)
        {
            await Task.Run(() => inst?.Stop());
            if (inst?.Exceptions != null)
                SensorsUpdated.Invoke(inst, EventArgs.Empty);
        }

        CurrentSensors.Clear();
    }


    public class SensorMeasuredEventArgs : EventArgs
    {
        public SensorMeasuredEventArgs(SensorInstance sensor, object? value, Exception? error = null)
        {
            Sensor = sensor;
            Value = value;
            Error = error;
        }

        public SensorMeasuredEventArgs(SensorInstance sensor, Exception? error = null)
        {
            Sensor = sensor;
            Error = error;
        }

        public SensorInstance Sensor { get; }
        public object? Value { get; }
        public Exception? Error { get; }
    }


    public class SensorStateChangedEventArgs : EventArgs
    {
        public SensorStateChangedEventArgs(SensorInstance sensor, State state)
        {
            Sensor = sensor;
            State = state;
        }

        public SensorInstance Sensor { get; }
        public State State { get; }
    }


    public class SensorInstance
    {
        public readonly SensorModel configuration;
        private List<Exception> _exceptions = new();
        private List<Timer> _schedulers = new();


        private SensorImplementation? _sensor;

        public SensorInstance(SensorModel configuration)
        {
            try
            {
                this.configuration = configuration;
                if (_sensor is IHumidity casted && configuration.HumidityInterval > 0)
                {
                    RegisterTimer<IHumidity, RelativeHumidity>(casted, configuration.HumidityInterval);
                }

                if (_sensor is ITemperature casted2 && configuration.HumidityInterval > 0)
                {
                    RegisterTimer<ITemperature, Temperature>(casted2, configuration.HumidityInterval);
                }
            }
            catch (Exception e)
            {
                _exceptions.Add(FailureException = e);
                throw;
            }

            CurrentState = State.Pending;
        }

        public State CurrentState { get; private set; } = State.Initializing;
        public IReadOnlyList<Exception> Exceptions => _exceptions.AsReadOnly();
        public Exception? FailureException { get; private set; }

        public event EventHandler<SensorMeasuredEventArgs> SensorValueReceived = delegate { };
        public event EventHandler<SensorStateChangedEventArgs> SensorStateChanged = delegate { };

        private void RegisterTimer<T, TUnit>(T casted, int interval) where T : ISensorCapability<TUnit>
        {
            var timer = new Timer(TimeSpan.FromSeconds(interval).Milliseconds);
            timer.Elapsed += (sender, args) =>
            {
                var value = casted.GetValue();
                SensorValueReceived.Invoke(this, new(this, value));
            };
            _schedulers.Add(timer);
        }


        public void Start()
        {
            try
            {
                CurrentState = State.Starting;
                SensorStateChanged.Invoke(this, new(this, CurrentState));
                _sensor = configuration.SensorConnection.CreateSensor();
                foreach (var timer in _schedulers)
                {
                    timer.Start();
                }

                CurrentState = State.Running;
                SensorStateChanged.Invoke(this, new(this, CurrentState));
            }
            catch (Exception e)
            {
                _exceptions.Add(FailureException = e);
            }
        }


        public void Stop()
        {
            try
            {
                CurrentState = State.Stopping;
                SensorStateChanged.Invoke(this, new(this, CurrentState));
                _sensor?.Dispose();
                foreach (var timer in _schedulers)
                {
                    timer.Dispose();
                }

                SensorValueReceived = delegate { };

                CurrentState = State.Stopped;
                SensorStateChanged.Invoke(this, new(this, CurrentState));
                SensorValueReceived = delegate { };
                SensorStateChanged = delegate { };
            }
            catch (Exception e)
            {
                _exceptions.Add(FailureException = e);
            }
        }
    }
}

internal record SensorModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Description { get; set; }
    public bool Enabled { get; set; } = true;
    public int HumidityInterval { get; set; } = 5;
    public int TemperatureInterval { get; set; } = 5;
    public SensorConnection SensorConnection { get; set; } = null!;

    public SensorModel DeepCopy()
    {
        return this with { SensorConnection = SensorConnection with { } };
    }
}

[Guid("0AABA153-E8ED-4A7C-AF1D-6D7C87D81FC6")]
public abstract class SensorImplementation : IDisposable
{
    public abstract void Dispose();
}

interface ISensorCapability<T>
{
    T GetValue();
}

interface ITemperature : ISensorCapability<Temperature>
{
}

internal interface IHumidity : ISensorCapability<RelativeHumidity>
{
}

internal interface IPressure
{
    Pressure GetPressure();
}

public abstract record SensorConnection
{
    public abstract SensorImplementation CreateSensor();
}

[Guid("8854A86A-F1E2-4A69-8EAC-0D7C2BFE772B")]
record Aht20SensorConnection : SensorConnection
{
    [DisplayName("Bus Id")] public int BusId { get; set; } = 1;

    public override Aht20SensorImplementation CreateSensor()
    {
        throw new NotImplementedException();
    }
}

class Aht20SensorImplementation : SensorImplementation, IHumidity, ITemperature
{
    private Aht20 _sensor;

    public Aht20SensorImplementation(Aht20SensorConnection connection)
    {
        _sensor = new(I2cDevice.Create(new(connection.BusId, Aht20.DefaultI2cAddress)));
    }

    RelativeHumidity ISensorCapability<RelativeHumidity>.GetValue()
    {
        return _sensor.GetHumidity();
    }

    Temperature ISensorCapability<Temperature>.GetValue()
    {
        return _sensor.GetTemperature();
    }

    public override void Dispose()
    {
        _sensor.Dispose();
    }
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