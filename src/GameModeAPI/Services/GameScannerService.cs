using GameModeAPI.Configuration;
using GameModeAPI.Mqtt;
using GameModeAPI.State;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameModeAPI.Detection;

public class GameScannerService : BackgroundService
{
    private readonly ILauncherScanner _launcherScanner;
    private readonly IProcessMonitor _processMonitor;
    private readonly IFullscreenDetector _fullscreenDetector;
    private readonly IGameMatcher _gameMatcher;
    private readonly IStateManager _stateManager;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly ICustomGamesLoader _customGamesLoader;
    private readonly DetectionSettings _settings;
    private readonly ILogger<GameScannerService> _logger;

    public GameScannerService(
        ILauncherScanner launcherScanner,
        IProcessMonitor processMonitor,
        IFullscreenDetector fullscreenDetector,
        IGameMatcher gameMatcher,
        IStateManager stateManager,
        IMqttPublisher mqttPublisher,
        ICustomGamesLoader customGamesLoader,
        IOptions<DetectionSettings> options,
        ILogger<GameScannerService> logger)
    {
        _launcherScanner = launcherScanner;
        _processMonitor = processMonitor;
        _fullscreenDetector = fullscreenDetector;
        _gameMatcher = gameMatcher;
        _stateManager = stateManager;
        _mqttPublisher = mqttPublisher;
        _customGamesLoader = customGamesLoader;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameScannerService is starting.");

        try
        {
            await _mqttPublisher.ConnectAsync();
            
            // Force publish the initial state so Home Assistant doesn't show "Unknown"
            _logger.LogInformation("Publishing initial Idle state to MQTT.");
            await _mqttPublisher.PublishGameModeAsync(false);
            await _mqttPublisher.PublishActiveGameAsync(GameState.Idle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker on startup.");
        }

        var customGames = _customGamesLoader.Load(_settings.CustomGamesPath);

        if (_settings.EnableLauncherScanning)
        {
            _launcherScanner.ScanAll();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.EnableLauncherScanning && 
                    _launcherScanner.NeedsRescan(_settings.LauncherRescanIntervalMinutes))
                {
                    _launcherScanner.ScanAll();
                    // Also reload custom games on the same interval
                    customGames = _customGamesLoader.Load(_settings.CustomGamesPath);
                }

                var candidates = _processMonitor.GetGameCandidates();
                var isFullscreen = _fullscreenDetector.IsFullscreen();

                var detected = _gameMatcher.Match(candidates, _launcherScanner.GameDatabase, customGames, isFullscreen);
                var (state, changed) = _stateManager.UpdateState(detected);

                if (changed)
                {
                    _logger.LogInformation("Publishing new state: IsGaming={IsGaming}, Game={GameName}", state.IsGaming, state.GameName);
                    await _mqttPublisher.PublishGameModeAsync(state.IsGaming);
                    await _mqttPublisher.PublishActiveGameAsync(state);
                }

                await Task.Delay(TimeSpan.FromSeconds(_settings.PollIntervalSeconds), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the polling cycle.");
                await Task.Delay(TimeSpan.FromSeconds(_settings.PollIntervalSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("GameScannerService is stopping.");
        await _mqttPublisher.DisposeAsync();
    }
}
