namespace GameDetect.Configuration;

public class MqttSettings
{
    public string Host { get; set; } = "homeassistant.local";
    public int Port { get; set; } = 1883;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseTls { get; set; } = false;
    public string ClientId { get; set; } = "GameDetect";
    public string DiscoveryPrefix { get; set; } = "homeassistant";
}

