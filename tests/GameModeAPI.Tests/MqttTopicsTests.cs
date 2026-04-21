using FluentAssertions;
using GameModeAPI.Mqtt;
using Xunit;

namespace GameModeAPI.Tests;

public class MqttTopicsTests
{
    [Fact]
    public void GameModeState_ReturnsCorrectTopic()
    {
        MqttTopics.GameModeState("test_dev").Should().Be("gamemodeapi/test_dev/game_mode/state");
    }

    [Fact]
    public void ActiveGameState_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameState("test_dev").Should().Be("gamemodeapi/test_dev/active_game/state");
    }

    [Fact]
    public void ActiveGameAttributes_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameAttributes("test_dev").Should().Be("gamemodeapi/test_dev/active_game/attributes");
    }

    [Fact]
    public void Availability_ReturnsCorrectTopic()
    {
        MqttTopics.Availability("test_dev").Should().Be("gamemodeapi/test_dev/availability");
    }

    [Fact]
    public void GameModeDiscovery_ReturnsCorrectTopic()
    {
        MqttTopics.GameModeDiscovery("ha", "test_dev").Should().Be("ha/binary_sensor/gamemodeapi_test_dev/game_mode/config");
    }

    [Fact]
    public void ActiveGameDiscovery_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameDiscovery("ha", "test_dev").Should().Be("ha/sensor/gamemodeapi_test_dev/active_game/config");
    }
}
