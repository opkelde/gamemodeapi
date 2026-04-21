using GameModeAPI.Detection.Models;

namespace GameModeAPI.Detection;

public interface IGameMatcher
{
    DetectedGame? Match(
        List<ProcessInfo> candidates, 
        IReadOnlyDictionary<string, KnownGame> gameDb, 
        List<CustomGameEntry> customGames,
        bool isFullscreen);
}
