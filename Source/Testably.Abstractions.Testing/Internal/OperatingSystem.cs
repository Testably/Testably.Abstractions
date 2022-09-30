using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class OperatingSystem
{
    public static bool IsWindows
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsNetFramework
    {
        get
        {
            _isNetFramework ??= RuntimeInformation
               .FrameworkDescription.StartsWith(".NET Framework");
            return _isNetFramework.Value;
        }
    }

    private static bool? _isNetFramework;
}