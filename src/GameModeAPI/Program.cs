using GameModeAPI.Configuration;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "GameModeAPI";
});

// Serilog
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
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
host.Run();
