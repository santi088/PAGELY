using Microsoft.Maui.Devices;

using Microsoft.Maui.Devices;

namespace PAGELY.Services;

/// <summary>
/// Centralized backend API configuration for all PAGELY services.
/// </summary>
public static class ApiConfiguration
{
    public const string PreferenceKey = "api_base_url";

    /// <summary>Address used by the Android emulator to reach the host machine.</summary>
    public const string AndroidEmulatorUrl = "http://10.0.2.2:5242";

    /// <summary>Address used by the iOS simulator to reach the host machine.</summary>
    public const string IosSimulatorUrl = "http://localhost:5242";

    /// <summary>Default address for a physical device on the same LAN.</summary>
    public const string PhysicalDeviceUrl = "http://192.168.100.166:5242";

    /// <summary>
    /// Platform-aware default. Physical devices should override this with the
    /// Fedora machine's LAN IP (or use 127.0.0.1 with `adb reverse`).
    /// </summary>
    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? PhysicalDeviceUrl
            : DeviceInfo.Platform == DevicePlatform.iOS
                ? IosSimulatorUrl
                : PhysicalDeviceUrl;

    public static string BaseUrl
    {
        get => Preferences.Default.Get(PreferenceKey, DefaultBaseUrl);
        set => Preferences.Default.Set(PreferenceKey, (value ?? string.Empty).Trim());
    }

    public static Uri BaseUri
    {
        get
        {
            var normalized = BaseUrl.TrimEnd('/') + "/";
            return new Uri(normalized);
        }
    }

    public static HttpClient CreateClient(TimeSpan? timeout = null)
    {
        return new HttpClient
        {
            BaseAddress = BaseUri,
            Timeout = timeout ?? TimeSpan.FromSeconds(15)
        };
    }
}
