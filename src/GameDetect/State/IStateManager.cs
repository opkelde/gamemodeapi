using GameDetect.Detection.Models;

namespace GameDetect.State;

public interface IStateManager
{
    (GameState State, bool Changed) UpdateState(DetectedGame? detected);
}

