using GameModeAPI.Configuration;
using GameModeAPI.Native;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameModeAPI.Detection;

public class FullscreenDetector : IFullscreenDetector
{
    private readonly DetectionSettings _settings;
    private readonly ILogger<FullscreenDetector> _logger;

    public FullscreenDetector(IOptions<DetectionSettings> options, ILogger<FullscreenDetector> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public bool IsFullscreen()
    {
        if (!_settings.EnableFullscreenDetection) return false;

        // Primary method
        var hr = NativeMethods.SHQueryUserNotificationState(out var state);
        if (hr == 0 && state == NativeMethods.QueryUserNotificationState.QUNS_RUNNING_D3D_FULL_SCREEN)
        {
            return true;
        }

        // Fallback method (Borderless Windowed)
        try
        {
            var hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd != IntPtr.Zero && NativeMethods.GetWindowRect(hWnd, out var rect))
            {
                var screenWidth = NativeMethods.GetSystemMetrics(0); // SM_CXSCREEN
                var screenHeight = NativeMethods.GetSystemMetrics(1); // SM_CYSCREEN

                // Check if the window is exactly the size of the primary screen
                if (rect.Left <= 0 && rect.Top <= 0 && 
                    rect.Right >= screenWidth && rect.Bottom >= screenHeight)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Failed to get foreground window rect for fallback fullscreen check");
        }

        return false;
    }
}
