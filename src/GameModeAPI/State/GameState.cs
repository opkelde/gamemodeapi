namespace GameModeAPI.State;

public record GameState(
    bool IsGaming,
    string? GameName,
    string? Launcher,
    string? AppId,
    string? ProcessName,
    bool IsFullscreen,
    DateTime? StartedAt
)
{
    public static readonly GameState Idle = new(false, null, null, null, null, false, null);
}
