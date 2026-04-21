namespace GameModeAPI.Detection.Models;

public record KnownGame(
    string Name,
    string ExecutableName,
    string? InstallPath,
    string Launcher,
    string? AppId
);
