using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.EADesktop;
using GameFinder.StoreHandlers.EADesktop.Crypto.Windows;
using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.GOG;
using GameFinder.StoreHandlers.Steam;
using GameFinder.StoreHandlers.Xbox;
using GameModeAPI.Detection.Models;
using Microsoft.Extensions.Logging;
using NexusMods.Paths;

namespace GameModeAPI.Detection;

public class LauncherScanner : ILauncherScanner
{
    private readonly ILogger<LauncherScanner> _logger;
    private Dictionary<string, KnownGame> _gameDb = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _lastScan = DateTime.MinValue;

    public LauncherScanner(ILogger<LauncherScanner> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, KnownGame> GameDatabase => _gameDb;

    public bool NeedsRescan(int intervalMinutes)
    {
        return (DateTime.UtcNow - _lastScan).TotalMinutes >= intervalMinutes;
    }

    public void ScanAll()
    {
        _logger.LogInformation("Scanning all launchers for installed games...");
        var db = new Dictionary<string, KnownGame>(StringComparer.OrdinalIgnoreCase);

        ScanSteam(db);
        ScanGOG(db);
        ScanEGS(db);
        ScanEA(db);
        ScanXbox(db);

        _gameDb = db;
        _lastScan = DateTime.UtcNow;
        _logger.LogInformation("Launcher scan complete. Found {Count} games.", db.Count);
    }

    private void AddGame(Dictionary<string, KnownGame> db, string name, string? path, string launcher, string? appId)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        try
        {
            // Find main executable. This is a heuristic.
            var exeFiles = Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly);
            if (exeFiles.Length == 0) return;

            // Pick largest exe or one matching name roughly (here just picking first for simplicity, ideally we'd rank them)
            var exeName = Path.GetFileName(exeFiles[0]);

            var game = new KnownGame(name, exeName, path, launcher, appId);
            db.TryAdd(exeName, game);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to resolve executable for {GameName} at {Path}", name, path);
        }
    }

    private void ScanSteam(Dictionary<string, KnownGame> db)
    {
        try
        {
            var handler = new SteamHandler(FileSystem.Shared, WindowsRegistry.Shared);
            var results = handler.FindAllGames();
            foreach (var result in results)
            {
                if (result.TryGetGame(out var game) && game != null)
                {
                    AddGame(db, game.Name, game.Path.ToString(), "Steam", game.AppId.Value.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Steam games");
        }
    }

    private void ScanGOG(Dictionary<string, KnownGame> db)
    {
        try
        {
            var handler = new GOGHandler(WindowsRegistry.Shared, FileSystem.Shared);
            var results = handler.FindAllGames();
            foreach (var result in results)
            {
                if (result.TryGetGame(out var game) && game != null)
                {
                    AddGame(db, game.Name, game.Path.ToString(), "GOG", game.Id.Value.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan GOG games");
        }
    }

    private void ScanEGS(Dictionary<string, KnownGame> db)
    {
        try
        {
            var handler = new EGSHandler(WindowsRegistry.Shared, FileSystem.Shared);
            var results = handler.FindAllGames();
            foreach (var result in results)
            {
                if (result.TryGetGame(out var game) && game != null)
                {
                    AddGame(db, game.DisplayName, game.InstallLocation.ToString(), "Epic", game.CatalogItemId.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Epic Games");
        }
    }

    private void ScanEA(Dictionary<string, KnownGame> db)
    {
        try
        {
            var handler = new EADesktopHandler(FileSystem.Shared, new HardwareInfoProvider());
            var results = handler.FindAllGames();
            foreach (var result in results)
            {
                if (result.TryGetGame(out var game) && game != null)
                {
                    AddGame(db, game.BaseSlug, game.BaseInstallPath.ToString(), "EA", game.EADesktopGameId.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan EA Desktop games. This can happen on hardware changes.");
        }
    }

    private void ScanXbox(Dictionary<string, KnownGame> db)
    {
        try
        {
            var handler = new XboxHandler(FileSystem.Shared);
            var results = handler.FindAllGames();
            foreach (var result in results)
            {
                if (result.TryGetGame(out var game) && game != null)
                {
                    AddGame(db, game.DisplayName, game.Path.ToString(), "Xbox", game.Id.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Xbox games");
        }
    }
}
