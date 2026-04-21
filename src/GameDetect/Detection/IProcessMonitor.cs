using GameDetect.Detection.Models;

namespace GameDetect.Detection;

public interface IProcessMonitor
{
    List<ProcessInfo> GetGameCandidates();
}

