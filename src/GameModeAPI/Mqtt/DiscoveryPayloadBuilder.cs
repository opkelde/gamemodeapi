using System.Text.Json;
using System.Text.Json.Serialization;
using GameModeAPI.Configuration;

namespace GameModeAPI.Mqtt;

public class DiscoveryPayloadBuilder
{
    private readonly ServiceSettings _serviceSettings;
    private readonly string _deviceId;

    public DiscoveryPayloadBuilder(ServiceSettings serviceSettings)
    {
        _serviceSettings = serviceSettings;
        _deviceId = serviceSettings.GetOrGenerateDeviceId();
    }

    private object BuildDeviceBlock()
    {
        return new
        {
            identifiers = new[] { $"gamemodeapi_{_deviceId}" },
            name = _serviceSettings.DeviceName,
            model = "GameModeAPI Client",
            manufacturer = "GameModeAPI",
            sw_version = "1.0.0" // Ideally read from assembly
        };
    }

    public string BuildGameModePayload()
    {
        var payload = new
        {
            name = "Game Mode",
            unique_id = $"gamemodeapi_{_deviceId}_game_mode",
            state_topic = MqttTopics.GameModeState(_deviceId),
            availability_topic = MqttTopics.Availability(_deviceId),
            device_class = "running",
            icon = "mdi:gamepad-variant",
            device = BuildDeviceBlock()
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string BuildActiveGamePayload()
    {
        var payload = new
        {
            name = "Active Game",
            unique_id = $"gamemodeapi_{_deviceId}_active_game",
            state_topic = MqttTopics.ActiveGameState(_deviceId),
            json_attributes_topic = MqttTopics.ActiveGameAttributes(_deviceId),
            availability_topic = MqttTopics.Availability(_deviceId),
            icon = "mdi:gamepad-variant-outline",
            device = BuildDeviceBlock()
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
