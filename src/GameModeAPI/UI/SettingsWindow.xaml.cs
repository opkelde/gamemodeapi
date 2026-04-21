using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using Microsoft.Win32;
using GameModeAPI.Configuration;
using Wpf.Ui.Appearance;
using System.Collections.ObjectModel;
using GameModeAPI.Detection.Models;

namespace GameModeAPI.UI;

public partial class SettingsWindow : Wpf.Ui.Controls.FluentWindow
{
    private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameModeAPI");
    private static readonly string AppSettingsPath = Path.Combine(AppDataDir, "config.json");
    private static readonly string CustomGamesPath = Path.Combine(AppDataDir, "custom_games.json");

    public ObservableCollection<CustomGameEntry> CustomGames { get; set; } = new();

    public SettingsWindow()
    {
        InitializeComponent();
        ApplicationThemeManager.Apply(this);
        SystemThemeWatcher.Watch(this);

        LoadSettings();
        LoadCustomGames();
        
        ChkAutostart.IsChecked = AutostartManager.IsAutostartEnabled();
    }

    private void LoadSettings()
    {
        if (!File.Exists(AppSettingsPath)) return;

        try
        {
            var json = File.ReadAllText(AppSettingsPath);
            var node = JsonNode.Parse(json);

            if (node != null && node["Mqtt"] != null)
            {
                TxtMqttHost.Text = node["Mqtt"]?["Host"]?.ToString() ?? "";
                TxtMqttPort.Text = node["Mqtt"]?["Port"]?.ToString() ?? "";
                TxtMqttUser.Text = node["Mqtt"]?["Username"]?.ToString() ?? "";
                TxtMqttPass.Password = node["Mqtt"]?["Password"]?.ToString() ?? "";
            }

            if (node != null && node["Service"]?["DeviceName"] != null)
            {
                TxtDeviceName.Text = node["Service"]?["DeviceName"]?.ToString() ?? "";
            }
        }
        catch { }
    }

    private void LoadCustomGames()
    {
        if (!File.Exists(CustomGamesPath)) return;
        try
        {
            var json = File.ReadAllText(CustomGamesPath);
            var root = JsonDocument.Parse(json).RootElement;
            if (root.TryGetProperty("games", out var gamesElement))
            {
                var games = JsonSerializer.Deserialize<CustomGameEntry[]>(gamesElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (games != null)
                {
                    foreach (var game in games) CustomGames.Add(game);
                }
            }
        }
        catch { }

        DgCustomGames.ItemsSource = CustomGames;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir);
            JsonNode node = File.Exists(AppSettingsPath) ? JsonNode.Parse(File.ReadAllText(AppSettingsPath))! : new JsonObject();

            if (node["Mqtt"] == null) node["Mqtt"] = new JsonObject();
            
            var rawHost = TxtMqttHost.Text?.Trim() ?? "";
            if (Uri.TryCreate(rawHost, UriKind.Absolute, out var uri) && 
                (rawHost.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || rawHost.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                rawHost = uri.Host;
            }
            else
            {
                rawHost = rawHost.TrimEnd('/');
            }
            
            node["Mqtt"]!["Host"] = rawHost;
            node["Mqtt"]!["Port"] = int.TryParse(TxtMqttPort.Text, out var port) ? port : 1883;
            node["Mqtt"]!["Username"] = TxtMqttUser.Text;
            node["Mqtt"]!["Password"] = TxtMqttPass.Password;

            if (node["Service"] == null) node["Service"] = new JsonObject();
            node["Service"]!["DeviceName"] = TxtDeviceName.Text;

            File.WriteAllText(AppSettingsPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            if (ChkAutostart.IsChecked == true) AutostartManager.EnableAutostart();
            else AutostartManager.DisableAutostart();

            MessageBox.Show("Settings saved successfully! Changes have been applied instantly.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSaveGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dir = Path.GetDirectoryName(CustomGamesPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var root = new { games = CustomGames };
            var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            File.WriteAllText(CustomGamesPath, json);

            MessageBox.Show("Custom games saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save custom games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnBackup_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Zip Archive|*.zip", FileName = "GameModeAPI_Backup.zip" };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (File.Exists(dialog.FileName)) File.Delete(dialog.FileName);
                BackupManager.BackupConfiguration(dialog.FileName);
                MessageBox.Show("Backup created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Backup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private void BtnRestore_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Zip Archive|*.zip" };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                BackupManager.RestoreConfiguration(dialog.FileName);
                LoadSettings();
                CustomGames.Clear();
                LoadCustomGames();
                MessageBox.Show("Configuration restored successfully! Please restart the service.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Restore failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private void BtnLightMode_Click(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.Acrylic);
        ApplicationThemeManager.Apply(this);
    }

    private void BtnDarkMode_Click(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.Acrylic);
        ApplicationThemeManager.Apply(this);
    }
}
