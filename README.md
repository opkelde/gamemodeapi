# 🎮 GameModeAPI

A standalone Windows desktop companion application that detects running games on your PC and publishes your active game status to [Home Assistant](https://www.home-assistant.io) via MQTT.

## What It Does

GameModeAPI quietly monitors your Windows PC in the background for running games and automatically reports two entities to Home Assistant:

- **Game Mode** (`binary_sensor`) — `ON` when any game is detected, `OFF` otherwise.
- **Active Game** (`sensor`) — The name of the currently running game, with extended attributes like the launcher used, fullscreen status, and session start time.

## Use Cases

- 🎮 **Gaming Mode Automations** — Automatically dim the lights, switch your room to a gaming scene, or mute notifications the moment you start playing.
- 📊 **Game Session Tracking** — Track exactly which games you play and build a history of your gaming sessions in Home Assistant.
- 🔔 **Smart Notifications** — Prevent text-to-speech announcements or push notifications from firing while you're in an intense gaming session.
- 💡 **RGB Sync** — Trigger custom WLED lighting effects based on whether you're actively gaming.

## How It Works

GameModeAPI uses multiple passive detection methods for reliable game identification:

1. **Launcher Scanning** — Automatically discovers installed games from Steam, GOG, Epic Games Store, EA Desktop, and Xbox Game Pass.
2. **Process Monitoring** — Polls running processes against a known game database.
3. **Fullscreen Detection** — Uses Windows APIs to detect D3D fullscreen applications.
4. **Custom Game List** — Allows you to define custom games directly in the UI for standalone or non-launcher titles.

---

## 🛠 Prerequisites: Setting up MQTT on Home Assistant

To use GameModeAPI, your Home Assistant instance must have an MQTT broker running. The easiest way is to use the official **Mosquitto broker** add-on.

If you don't have MQTT set up yet, follow these quick steps:

1. Open your Home Assistant dashboard and navigate to **Settings** > **Add-ons**.
2. Click the **Add-on Store** button in the bottom right corner.
3. Search for **"Mosquitto broker"**, select the official add-on, and click **Install**.
4. Once installed, go to the **Info** tab, toggle on **Start on boot** and **Watchdog**, then click **Start**.
5. Navigate to **Settings** > **Devices & services**. Home Assistant should automatically pop up a "New device discovered" notification for MQTT. Click **Configure** and then **Submit**.

---

## 🚀 Quick Start Guide

Setting up GameModeAPI on your gaming PC is simple and doesn't require editing any text files:

1. **Download & Run**: Download the latest release of `GameModeAPI.exe` and run it. You will see a gaming controller icon appear in your Windows System Tray (bottom right corner).
2. **Open Settings**: Double-click the tray icon to open the beautiful, modern **GameModeAPI Settings** window.
3. **Configure MQTT**: 
   - **Host**: Enter the IP address of your Home Assistant instance (e.g., `192.168.1.100` or `homeassistant.local`). You don't need to add `http://` or ports!
   - **Username / Password**: Enter the credentials for your MQTT broker (often the same as your Home Assistant login if using Mosquitto).
4. **Device Name**: Customize the name of your PC (e.g., `Gaming PC`). This is how it will show up in Home Assistant.
5. **Autostart**: Check the "Start with Windows" box if you want GameModeAPI to run automatically in the background when you turn on your PC.
6. **Save**: Click **Save Settings**. GameModeAPI will instantly connect to Home Assistant without needing a restart!

Your Game Mode entities will automatically appear in Home Assistant under the MQTT integration as a new device!

---

## 🔧 Custom Games

If you play games that are not installed via a standard launcher (e.g., standalone executables, emulators), you can add them manually:
1. Open the GameModeAPI Settings window.
2. Navigate to the **Custom Games** section.
3. Add the exact executable name (e.g., `minecraft.exe`) and the Display Name you want sent to Home Assistant.
4. Click **Save Games**.

---

## ⚙️ Advanced: Configuration Data

GameModeAPI stores all of your settings and custom games safely in your Windows AppData folder:
`%AppData%\GameModeAPI\`

You can backup and restore your configurations easily using the built-in Backup buttons in the Settings window.

---

## License

This project is licensed under the GPL-3.0 License — see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [HASS.Agent](https://github.com/hass-agent/HASS.Agent) — The Windows companion app for Home Assistant that originally inspired this project.
- [GameFinder](https://github.com/erri120/GameFinder) — Game installation detection library.
- [MQTTnet](https://github.com/dotnet/MQTTnet) — The MQTT library used for seamless broker communication.
- [WPF-UI](https://github.com/lepoco/wpfui) — The library powering the beautiful modern interface.
