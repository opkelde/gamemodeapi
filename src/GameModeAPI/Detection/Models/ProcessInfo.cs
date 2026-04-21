namespace GameModeAPI.Detection.Models;

public record ProcessInfo(
    string ProcessName,
    string? FilePath,
    string? WindowTitle,
    DateTime StartTime,
    IntPtr WindowHandle
);
