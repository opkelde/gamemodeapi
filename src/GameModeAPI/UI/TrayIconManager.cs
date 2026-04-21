using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace GameModeAPI.UI;

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    public TrayIconManager()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "GameModeAPI"
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Configuration", null, (s, e) => OpenSettings());
        contextMenu.Items.Add("Restart Service", null, (s, e) => RestartService());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Uninstall", null, (s, e) => Uninstall());
        contextMenu.Items.Add("Exit", null, (s, e) => Exit());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => OpenSettings();
    }

    private void OpenSettings()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var window = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
            if (window != null)
            {
                if (window.WindowState == System.Windows.WindowState.Minimized)
                    window.WindowState = System.Windows.WindowState.Normal;
                window.Activate();
            }
            else
            {
                new SettingsWindow().Show();
            }
        });
    }

    private void RestartService()
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrEmpty(exePath))
        {
            System.Diagnostics.Process.Start(exePath);
        }
        Application.Current?.Shutdown();
    }

    private void Uninstall()
    {
        var result = MessageBox.Show("Are you sure you want to uninstall? This will remove the autostart registry key and exit the application.", "Uninstall GameModeAPI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            Configuration.AutostartManager.DisableAutostart();
            Application.Current?.Shutdown();
        }
    }

    private void Exit()
    {
        Application.Current?.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
