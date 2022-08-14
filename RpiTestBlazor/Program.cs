using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration.CommandLine;
using RpiTestBlazor.Data;
using RpiTestBlazor.Services;
using RpiTestBlazor.Services.Sensor;


if (args.Contains("--request-debugger") && !Debugger.IsAttached)
    Debugger.Launch();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    EnvironmentName = "Development"
});


builder.WebHost.ConfigureKestrel(options => { options.Listen(IPAddress.Parse("127.0.0.1"), 5003); });
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<SensorManagementService>();
builder.Services.AddSingleton<WeatherForecastService>();
//builder.Services.AddSingleton<DhtService>();
//builder.Services.AddHostedService<DhtService>(provider => provider.GetRequiredService<DhtService>());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


app.Run();
