using GameModeAPI.State;

namespace GameModeAPI.Mqtt;

public interface IMqttPublisher : IAsyncDisposable
{
    Task ConnectAsync();
    Task PublishGameModeAsync(bool isGaming);
    Task PublishActiveGameAsync(GameState state);
    Task PublishAvailabilityAsync(string status);
}
