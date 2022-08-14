using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace GN.Klima.IoT.Services;

public class ConfigurationService
{
    private string _configurationFilePath;

    public ConfigurationService()
    {
        _configurationFilePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
    }

    
    
    public void Load(string key, Action<Stream?> consumer)
    {
        var filename = WebUtility.HtmlEncode(key);
        var filepath = Path.Combine(_configurationFilePath, filename);
        

        if (File.Exists(filepath))
        {
            consumer(null);
            return;
        }
        
        using Stream parameterStream = File.OpenRead(filepath);
        consumer(parameterStream);
    }

    public void Save(string key, Stream value)
    {
        var filename = WebUtility.HtmlEncode(key);
        var filepath = Path.Combine(_configurationFilePath, filename);
        var stream = File.Create(filepath);
        stream.Flush(true);
    }
}