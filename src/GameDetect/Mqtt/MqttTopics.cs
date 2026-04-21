namespace GameDetect.Mqtt;

public static class MqttTopics
{
    public static string GameModeState(string deviceId) => $"gamedetect/{deviceId}/game_mode/state";
    public static string ActiveGameState(string deviceId) => $"gamedetect/{deviceId}/active_game/state";
    public static string ActiveGameAttributes(string deviceId) => $"gamedetect/{deviceId}/active_game/attributes";
    public static string Availability(string deviceId) => $"gamedetect/{deviceId}/availability";

    public static string GameModeDiscovery(string prefix, string deviceId) => 
        $"{prefix}/binary_sensor/gamedetect_{deviceId}/game_mode/config";
    public static string ActiveGameDiscovery(string prefix, string deviceId) => 
        $"{prefix}/sensor/gamedetect_{deviceId}/active_game/config";
}

