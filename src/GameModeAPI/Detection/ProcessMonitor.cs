using System.ComponentModel;
using System.Diagnostics;
using GameModeAPI.Detection.Models;
using Microsoft.Extensions.Logging;

namespace GameModeAPI.Detection;

public class ProcessMonitor : IProcessMonitor
{
    private readonly ILogger<ProcessMonitor> _logger;
    private static readonly HashSet<string> SystemProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "svchost", "csrss", "lsass", "services", "smss", "wininit",
        "winlogon", "dwm", "explorer", "taskhostw", "RuntimeBroker",
        "SearchHost", "ShellExperienceHost", "conhost", "System",
        "Idle", "Registry", "fontdrvhost", "sihost", "ctfmon", "TextInputHost"
    };

    public ProcessMonitor(ILogger<ProcessMonitor> logger)
    {
        _logger = logger;
    }

    public List<ProcessInfo> GetGameCandidates()
    {
        var candidates = new List<ProcessInfo>();
        var processes = Process.GetProcesses();

        foreach (var proc in processes)
        {
            try
            {
                if (SystemProcesses.Contains(proc.ProcessName)) continue;
                if (proc.MainWindowHandle == IntPtr.Zero) continue;

                var info = new ProcessInfo(
                    proc.ProcessName,
                    proc.MainModule?.FileName,
                    proc.MainWindowTitle,
                    proc.StartTime,
                    proc.MainWindowHandle
                );
                candidates.Add(info);
            }
            catch (Win32Exception)
            {
                // Process is likely elevated and we aren't, or similar access issue.
                // We'll skip grabbing MainModule if it fails, but we can still grab name/title.
                try
                {
                    if (proc.MainWindowHandle != IntPtr.Zero)
                    {
                        var info = new ProcessInfo(
                            proc.ProcessName,
                            null, // FilePath inaccessible
                            proc.MainWindowTitle,
                            proc.StartTime,
                            proc.MainWindowHandle
                        );
                        candidates.Add(info);
                    }
                }
                catch { /* Ignore */ }
            }
            catch (InvalidOperationException)
            {
                // Process exited mid-check
            }
            finally
            {
                proc.Dispose();
            }
        }

        return candidates;
    }
}
