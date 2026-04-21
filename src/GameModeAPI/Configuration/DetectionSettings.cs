namespace GameModeAPI.Configuration;

public class DetectionSettings
{
    public int PollIntervalSeconds { get; set; } = 5;
    public int DebounceSeconds { get; set; } = 10;
    public bool EnableFullscreenDetection { get; set; } = true;
    public bool EnableLauncherScanning { get; set; } = true;
    public string CustomGamesPath { get; set; } = "data/custom_games.json";
    public int LauncherRescanIntervalMinutes { get; set; } = 60;
}
