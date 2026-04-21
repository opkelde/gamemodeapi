using FluentAssertions;
using GameDetect.Mqtt;
using Xunit;

namespace GameDetect.Tests;

public class MqttTopicsTests
{
    [Fact]
    public void GameModeState_ReturnsCorrectTopic()
    {
        MqttTopics.GameModeState("test_dev").Should().Be("gamedetect/test_dev/game_mode/state");
    }

    [Fact]
    public void ActiveGameState_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameState("test_dev").Should().Be("gamedetect/test_dev/active_game/state");
    }

    [Fact]
    public void ActiveGameAttributes_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameAttributes("test_dev").Should().Be("gamedetect/test_dev/active_game/attributes");
    }

    [Fact]
    public void Availability_ReturnsCorrectTopic()
    {
        MqttTopics.Availability("test_dev").Should().Be("gamedetect/test_dev/availability");
    }

    [Fact]
    public void GameModeDiscovery_ReturnsCorrectTopic()
    {
        MqttTopics.GameModeDiscovery("ha", "test_dev").Should().Be("ha/binary_sensor/gamedetect_test_dev/game_mode/config");
    }

    [Fact]
    public void ActiveGameDiscovery_ReturnsCorrectTopic()
    {
        MqttTopics.ActiveGameDiscovery("ha", "test_dev").Should().Be("ha/sensor/gamedetect_test_dev/active_game/config");
    }
}

