using System.Text.Json.Serialization;

namespace GameModeAPI.Detection.Models;

public class CustomGameEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("executable")]
    public string Executable { get; set; } = string.Empty;

    [JsonPropertyName("match_window_title")]
    public string? MatchWindowTitle { get; set; }

    [JsonPropertyName("launcher")]
    public string Launcher { get; set; } = "Custom";
}

public class CustomGamesDatabase
{
    [JsonPropertyName("games")]
    public List<CustomGameEntry> Games { get; set; } = [];
}
