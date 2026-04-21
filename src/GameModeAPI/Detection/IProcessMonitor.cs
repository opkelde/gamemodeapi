using GameModeAPI.Detection.Models;

namespace GameModeAPI.Detection;

public interface IProcessMonitor
{
    List<ProcessInfo> GetGameCandidates();
}
