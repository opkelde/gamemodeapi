using System.Text.Json;
using GameDetect.Configuration;
using GameDetect.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace GameDetect.Mqtt;

public class MqttPublisher : IMqttPublisher
{
    private MqttSettings _mqttSettings;
    private ServiceSettings _serviceSettings;
    private readonly ILogger<MqttPublisher> _logger;
    private IManagedMqttClient? _client;
    private readonly string _deviceId;
    private DiscoveryPayloadBuilder _discoveryBuilder;

    public MqttPublisher(
        IOptionsMonitor<MqttSettings> mqttOptionsMonitor,
        IOptionsMonitor<ServiceSettings> serviceOptionsMonitor,
        ILogger<MqttPublisher> logger)
    {
        _mqttSettings = mqttOptionsMonitor.CurrentValue;
        mqttOptionsMonitor.OnChange(settings =>
        {
            logger.LogInformation("MQTT settings changed. Reconnecting...");
            _mqttSettings = settings;
            _ = ReconnectAsync();
        });
        
        _serviceSettings = serviceOptionsMonitor.CurrentValue;
        serviceOptionsMonitor.OnChange(settings =>
        {
            logger.LogInformation("Service settings changed. Updating device name...");
            _serviceSettings = settings;
            _discoveryBuilder = new DiscoveryPayloadBuilder(_serviceSettings);
            if (_client != null && _client.IsStarted)
            {
                _ = PublishDiscoveryAsync();
            }
        });
        _logger = logger;
        _deviceId = _serviceSettings.GetOrGenerateDeviceId();
        _discoveryBuilder = new DiscoveryPayloadBuilder(_serviceSettings);
    }

    public async Task ConnectAsync()
    {
        var factory = new MqttFactory();
        _client = factory.CreateManagedMqttClient();

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
        await ReconnectAsync();
    }

    private async Task ReconnectAsync()
    {
        if (_client == null) return;
        if (_client.IsStarted)
        {
            await _client.StopAsync();
        }

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

        var managedOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(clientOptionsBuilder.Build())
            .Build();

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

