using System.IO;
using System.IO.Compression;

namespace GameModeAPI.Configuration;

public static class BackupManager
{
    private static readonly string[] FilesToBackup = {
        "appsettings.json",
        "appsettings.Development.json",
        "data/custom_games.json"
    };

    public static void BackupConfiguration(string zipPath)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var file in FilesToBackup)
        {
            var fullPath = Path.Combine(basePath, file);
            if (File.Exists(fullPath))
            {
                archive.CreateEntryFromFile(fullPath, file, CompressionLevel.Optimal);
            }
        }
    }

    public static void RestoreConfiguration(string zipPath)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            var destPath = Path.Combine(basePath, entry.FullName);
            var dir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            entry.ExtractToFile(destPath, overwrite: true);
        }
    }
}
