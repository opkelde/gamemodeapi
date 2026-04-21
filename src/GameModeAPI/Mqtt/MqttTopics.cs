namespace GameModeAPI.Mqtt;

public static class MqttTopics
{
    public static string GameModeState(string deviceId) => $"gamemodeapi/{deviceId}/game_mode/state";
    public static string ActiveGameState(string deviceId) => $"gamemodeapi/{deviceId}/active_game/state";
    public static string ActiveGameAttributes(string deviceId) => $"gamemodeapi/{deviceId}/active_game/attributes";
    public static string Availability(string deviceId) => $"gamemodeapi/{deviceId}/availability";

    public static string GameModeDiscovery(string prefix, string deviceId) => 
        $"{prefix}/binary_sensor/gamemodeapi_{deviceId}/game_mode/config";
    public static string ActiveGameDiscovery(string prefix, string deviceId) => 
        $"{prefix}/sensor/gamemodeapi_{deviceId}/active_game/config";
}
