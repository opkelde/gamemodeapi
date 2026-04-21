using GameModeAPI.Detection.Models;

namespace GameModeAPI.State;

public interface IStateManager
{
    (GameState State, bool Changed) UpdateState(DetectedGame? detected);
}
