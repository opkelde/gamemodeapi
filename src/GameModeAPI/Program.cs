using GameModeAPI.Configuration;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace GameModeAPI;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Windows Service support
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "GameModeAPI";
        });

        // Serilog
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.File("logs/gamemodeapi-.log", rollingInterval: RollingInterval.Day));

        // Configuration binding
        builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("Mqtt"));
        builder.Services.Configure<DetectionSettings>(builder.Configuration.GetSection("Detection"));
        builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Service"));

        // Services (DI registration)
        builder.Services.AddSingleton<GameModeAPI.Detection.ILauncherScanner, GameModeAPI.Detection.LauncherScanner>();
        builder.Services.AddSingleton<GameModeAPI.Detection.IProcessMonitor, GameModeAPI.Detection.ProcessMonitor>();
        builder.Services.AddSingleton<GameModeAPI.Detection.IFullscreenDetector, GameModeAPI.Detection.FullscreenDetector>();
        builder.Services.AddSingleton<GameModeAPI.Detection.IGameMatcher, GameModeAPI.Detection.GameMatcher>();
        builder.Services.AddSingleton<GameModeAPI.Detection.ICustomGamesLoader, GameModeAPI.Detection.CustomGamesLoader>();
        builder.Services.AddSingleton<GameModeAPI.State.IStateManager, GameModeAPI.State.StateManager>();
        builder.Services.AddSingleton<GameModeAPI.Mqtt.IMqttPublisher, GameModeAPI.Mqtt.MqttPublisher>();
        builder.Services.AddHostedService<GameModeAPI.Detection.GameScannerService>();

        var host = builder.Build();

        // Start background host
        _ = host.StartAsync();

        // Start WPF Application
        var app = new System.Windows.Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
        
        GameModeAPI.UI.TrayIconManager? trayIcon = null;
        
        app.Startup += (s, e) => 
        {
            trayIcon = new GameModeAPI.UI.TrayIconManager();
            
            // Check if MqttHost is default
            var mqttHost = builder.Configuration.GetSection("Mqtt")["Host"];
            if (string.IsNullOrEmpty(mqttHost) || mqttHost == "homeassistant.local")
            {
                new GameModeAPI.UI.SettingsWindow().Show();
            }
        };

        app.Exit += async (s, e) => 
        {
            trayIcon?.Dispose();
            await host.StopAsync();
            host.Dispose();
        };

        app.Run();
    }
}
