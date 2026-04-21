using System.IO;
using System.Text.Json;
using GameModeAPI.Detection.Models;
using Microsoft.Extensions.Logging;

namespace GameModeAPI.Detection;

public interface ICustomGamesLoader
{
    List<CustomGameEntry> Load(string path);
}

public class CustomGamesLoader : ICustomGamesLoader
{
    private readonly ILogger<CustomGamesLoader> _logger;

    public CustomGamesLoader(ILogger<CustomGamesLoader> logger)
    {
        _logger = logger;
    }

    public List<CustomGameEntry> Load(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                _logger.LogWarning("Custom games file not found at {Path}", path);
                return new List<CustomGameEntry>();
            }

            var json = File.ReadAllText(path);
            var db = JsonSerializer.Deserialize<CustomGamesDatabase>(json);
            
            if (db?.Games != null)
            {
                _logger.LogInformation("Loaded {Count} custom games from {Path}", db.Games.Count, path);
                return db.Games;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load custom games from {Path}", path);
        }

        return new List<CustomGameEntry>();
    }
}
