# 🎮 GameModeAPI

A standalone Windows service that detects running games on your PC and publishes game status to [Home Assistant](https://www.home-assistant.io) via MQTT.

## What It Does

GameModeAPI monitors your Windows PC for running games and reports two entities to Home Assistant:

- **Game Mode** (`binary_sensor`) — `ON` when any game is detected, `OFF` otherwise
- **Active Game** (`sensor`) — The name of the currently running game, with attributes like launcher, fullscreen status, and session start time

## Use Cases

- 🎮 **Gaming Mode Automations** — Dim the lights, switch to a gaming scene, or mute notifications when you start playing
- 📊 **Game Session Tracking** — Track which games you play and when
- 🔔 **Smart Notifications** — Suppress Home Assistant notifications while gaming
- 💡 **RGB Sync** — Trigger lighting effects based on whether you're gaming

## How It Works

GameModeAPI uses multiple detection methods for reliable game identification:

1. **Launcher Scanning** — Discovers installed games from Steam, GOG, Epic Games Store, EA Desktop, and Xbox Game Pass
2. **Process Monitoring** — Polls running processes and matches against the known game database
3. **Fullscreen Detection** — Uses Windows APIs to detect D3D fullscreen applications
4. **Custom Game List** — User-defined games for standalone/non-launcher titles

## Integration with Home Assistant

GameModeAPI publishes its state to your Home Assistant instance using MQTT discovery. It appears as a standalone device in the Home Assistant MQTT integration automatically.

## Requirements

- Windows 10/11
- .NET 8 Runtime
- MQTT Broker (e.g., Mosquitto)
- Home Assistant with MQTT integration configured

## Quick Start

1. Download the latest release
2. Edit `appsettings.json` with your MQTT broker details
3. Run `GameModeAPI.exe` (or install as a Windows Service)
4. Game Mode entities will automatically appear in Home Assistant

## Configuration

See `appsettings.json` for all configuration options including:
- MQTT broker connection details
- Polling interval
- Debounce timing
- Fullscreen detection toggle
- Custom game definitions (via `data/custom_games.json`)

## Windows Service Installation

You can install GameModeAPI as a background Windows Service:

```powershell
sc.exe create "GameModeAPI" binPath= "C:\path\to\GameModeAPI.exe" start= auto
sc.exe start "GameModeAPI"
```

## License

This project is licensed under the GPL-3.0 License — see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue first to discuss what you would like to change.

## Acknowledgments

- [HASS.Agent](https://github.com/hass-agent/HASS.Agent) — The Windows companion app for Home Assistant that inspired this project
- [MQTTnet](https://github.com/dotnet/MQTTnet) — The MQTT library used for broker communication
- [Home Assistant](https://www.home-assistant.io) — The open-source home automation platform
- [GameFinder](https://github.com/erri120/GameFinder) — Game installation detection library
