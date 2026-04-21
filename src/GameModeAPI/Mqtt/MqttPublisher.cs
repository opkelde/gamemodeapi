using System.Text.Json;
using GameModeAPI.Configuration;
using GameModeAPI.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace GameModeAPI.Mqtt;

public class MqttPublisher : IMqttPublisher
{
    private readonly MqttSettings _mqttSettings;
    private readonly ServiceSettings _serviceSettings;
    private readonly ILogger<MqttPublisher> _logger;
    private IManagedMqttClient? _client;
    private readonly string _deviceId;
    private readonly DiscoveryPayloadBuilder _discoveryBuilder;

    public MqttPublisher(
        IOptions<MqttSettings> mqttOptions,
        IOptions<ServiceSettings> serviceOptions,
        ILogger<MqttPublisher> logger)
    {
        _mqttSettings = mqttOptions.Value;
        _serviceSettings = serviceOptions.Value;
        _logger = logger;
        _deviceId = _serviceSettings.GetOrGenerateDeviceId();
        _discoveryBuilder = new DiscoveryPayloadBuilder(_serviceSettings);
    }

    public async Task ConnectAsync()
    {
        var factory = new MqttFactory();
        _client = factory.CreateManagedMqttClient();

        var clientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttSettings.Host, _mqttSettings.Port)
            .WithClientId(_mqttSettings.ClientId)
            .WithWillTopic(MqttTopics.Availability(_deviceId))
            .WithWillPayload("offline")
            .WithWillRetain(true);

        if (!string.IsNullOrWhiteSpace(_mqttSettings.Username))
        {
            clientOptionsBuilder.WithCredentials(_mqttSettings.Username, _mqttSettings.Password);
        }

        if (_mqttSettings.UseTls)
        {
            clientOptionsBuilder.WithTlsOptions(o => o.UseTls());
        }

        var clientOptions = clientOptionsBuilder.Build();

        var managedOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(clientOptions)
            .Build();

        _client.ConnectedAsync += async e =>
        {
            _logger.LogInformation("Connected to MQTT Broker.");
            await PublishDiscoveryAsync();
            await PublishAvailabilityAsync("online");
        };

        _client.DisconnectedAsync += e =>
        {
            _logger.LogWarning("Disconnected from MQTT Broker.");
            return Task.CompletedTask;
        };

        _client.ConnectingFailedAsync += e =>
        {
            _logger.LogError(e.Exception, "MQTT Connection failed.");
            return Task.CompletedTask;
        };

        _logger.LogInformation("Starting MQTT client...");
        await _client.StartAsync(managedOptions);
    }

    private async Task PublishDiscoveryAsync()
    {
        if (_client == null) return;

        var gameModeConfig = _discoveryBuilder.BuildGameModePayload();
        var activeGameConfig = _discoveryBuilder.BuildActiveGamePayload();

        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.GameModeDiscovery(_mqttSettings.DiscoveryPrefix, _deviceId))
            .WithPayload(gameModeConfig)
            .WithRetainFlag(true)
            .Build());

        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.ActiveGameDiscovery(_mqttSettings.DiscoveryPrefix, _deviceId))
            .WithPayload(activeGameConfig)
            .WithRetainFlag(true)
            .Build());
            
        _logger.LogInformation("Published MQTT Discovery payloads.");
    }

    public async Task PublishGameModeAsync(bool isGaming)
    {
        if (_client == null) return;

        var payload = isGaming ? "ON" : "OFF";
        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.GameModeState(_deviceId))
            .WithPayload(payload)
            .WithRetainFlag(true)
            .Build());
            
        _logger.LogDebug("Published Game Mode: {State}", payload);
    }

    public async Task PublishActiveGameAsync(GameState state)
    {
        if (_client == null) return;

        var namePayload = state.IsGaming && !string.IsNullOrWhiteSpace(state.GameName) ? state.GameName : "None";
        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.ActiveGameState(_deviceId))
            .WithPayload(namePayload)
            .WithRetainFlag(true)
            .Build());

        var attributesPayload = JsonSerializer.Serialize(new
        {
            launcher = state.Launcher,
            app_id = state.AppId,
            process_name = state.ProcessName,
            is_fullscreen = state.IsFullscreen,
            started_at = state.StartedAt?.ToString("o")
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.ActiveGameAttributes(_deviceId))
            .WithPayload(attributesPayload)
            .WithRetainFlag(true)
            .Build());
            
        _logger.LogDebug("Published Active Game: {Name}", namePayload);
    }

    public async Task PublishAvailabilityAsync(string status)
    {
        if (_client == null) return;

        await _client.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(MqttTopics.Availability(_deviceId))
            .WithPayload(status)
            .WithRetainFlag(true)
            .Build());
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            await PublishAvailabilityAsync("offline");
            
            // Allow time for the offline message to be queued and sent before stopping
            await Task.Delay(500); 

            await _client.StopAsync();
            _client.Dispose();
            _logger.LogInformation("MQTT client stopped.");
        }
    }
}
