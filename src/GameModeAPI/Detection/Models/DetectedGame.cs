namespace GameModeAPI.Detection.Models;

public record DetectedGame(
    string Name,
    string Launcher,
    string? AppId,
    string ProcessName,
    bool IsFullscreen,
    DateTime StartedAt
);
