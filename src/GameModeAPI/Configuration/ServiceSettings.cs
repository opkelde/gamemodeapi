using System.Security.Cryptography;
using System.Text;

namespace GameModeAPI.Configuration;

public class ServiceSettings
{
    public string DeviceId { get; set; } = "auto";
    public string DeviceName { get; set; } = "Gaming PC";
    public string LogLevel { get; set; } = "Information";

    public string GetOrGenerateDeviceId()
    {
        if (!string.Equals(DeviceId, "auto", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(DeviceId))
        {
            return DeviceId;
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
        return Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
    }
}
