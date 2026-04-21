using GameDetect.Detection.Models;

namespace GameDetect.Detection;

public interface ILauncherScanner
{
    IReadOnlyDictionary<string, KnownGame> GameDatabase { get; }
    void ScanAll();
    bool NeedsRescan(int intervalMinutes);
}

