using GameDetect.Configuration;
using GameDetect.Detection.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameDetect.State;

public class StateManager : IStateManager
{
    private readonly DetectionSettings _settings;
    private readonly ILogger<StateManager> _logger;

    private GameState _currentState = GameState.Idle;
    private GameState _pendingState = GameState.Idle;
    private DateTime? _pendingStateStarted;

    public StateManager(IOptions<DetectionSettings> options, ILogger<StateManager> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public (GameState State, bool Changed) UpdateState(DetectedGame? detected)
    {
        var candidateState = detected != null
            ? new GameState(
                true, 
                detected.Name, 
                detected.Launcher, 
                detected.AppId, 
                detected.ProcessName, 
                detected.IsFullscreen, 
                detected.StartedAt)
            : GameState.Idle;

        // Fast path: no change at all
        if (candidateState == _currentState)
        {
            if (_pendingStateStarted != null)
            {
                _logger.LogDebug("Candidate state matches current state. Clearing pending state.");
            }
            _pendingState = _currentState;
            _pendingStateStarted = null;
            return (_currentState, false);
        }

        // Handle quick attribute updates (e.g. Fullscreen toggle on the SAME game)
        // We shouldn't debounce this, we want immediate update.
        if (_currentState.IsGaming && candidateState.IsGaming &&
            _currentState.GameName == candidateState.GameName &&
            _currentState.ProcessName == candidateState.ProcessName)
        {
            _logger.LogInformation("Game attributes updated: {GameName} (Fullscreen: {IsFullscreen})", 
                candidateState.GameName, candidateState.IsFullscreen);
            
            _currentState = candidateState;
            _pendingState = candidateState;
            _pendingStateStarted = null;
            return (_currentState, true);
        }

        // New state detected
        if (candidateState != _pendingState)
        {
            _logger.LogDebug("New pending state detected: {GameName}. Starting debounce timer.", 
                candidateState.GameName ?? "Idle");
            _pendingState = candidateState;
            _pendingStateStarted = DateTime.UtcNow;
            return (_currentState, false);
        }

        // Wait for debounce period
        if (_pendingStateStarted.HasValue)
        {
            var elapsedSeconds = (DateTime.UtcNow - _pendingStateStarted.Value).TotalSeconds;
            if (elapsedSeconds >= _settings.DebounceSeconds)
            {
                _logger.LogInformation("State transition confirmed after {DebounceSeconds}s: {OldState} -> {NewState}", 
                    _settings.DebounceSeconds,
                    _currentState.GameName ?? "Idle",
                    _pendingState.GameName ?? "Idle");

                _currentState = _pendingState;
                _pendingStateStarted = null;
                return (_currentState, true);
            }
        }

        return (_currentState, false);
    }
}

