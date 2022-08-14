using System.ComponentModel;
using System.Device.I2c;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Iot.Device.Ahtxx;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace GN.Klima.IoT.Services;

public interface ISensorProvider
{

    public object QuerySensor(Type type);
    public sealed TValue QuerySensor<TValue>() => (TValue)QuerySensor(typeof(TValue));

    public IComponent CreateBlazorSettingsComponent();

    public void Start();
}

[SupportedUnit(typeof(RelativeHumidity)), SupportedUnit(typeof(Temperature))]
public class Aht20SensorProvider : ISensorProvider, IDisposable
{
    private readonly ConfigurationService _configuration;
    private readonly ILogger<Aht20SensorProvider> _logger;
    private const string Key = "AHT20";
    private Aht20? _aht20;

    public Aht20SensorProvider(ConfigurationService configuration, ILogger<Aht20SensorProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Aht20ProviderOptions? CurrentConfiguration { get; private set; }

    public void Dispose()
    {
        _aht20?.Dispose();
    }
    
    public IComponent CreateBlazorSettingsComponent()
    {
        throw new NotImplementedException();
    }


    public void Start()
    {
        _ = CurrentConfiguration == null &&
            LoadConfiguration() &&
            SaveConfiguration(new Aht20ProviderOptions());
    }
    
    

    private bool LoadConfiguration()
    {
        try
        {
            _configuration.Load(Key, stream =>
            {
                if (stream != null)
                {
                    CurrentConfiguration = JsonSerializer.Deserialize<Aht20ProviderOptions>(stream);
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading configuration");
        }

        return CurrentConfiguration != null;
    }

    public bool SaveConfiguration(Aht20ProviderOptions configToSave)
    {
        try
        {
            var mem = new MemoryStream();
            JsonSerializer.Serialize(mem, configToSave);
            _configuration.Save(Key, mem);
            CurrentConfiguration = configToSave;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving configuration");
        }

        return true;
    }
    


    public class Aht20ProviderOptions
    {
        public int BusId;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SupportedUnitAttribute : Attribute
{
    public SupportedUnitAttribute(Type unit)
    {
        Unit = unit;
    }

    public Type Unit { get; }
}