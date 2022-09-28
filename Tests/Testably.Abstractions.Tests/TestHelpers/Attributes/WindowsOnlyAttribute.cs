using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
///     <see cref="OSPlatform.Windows" />.
/// </summary>
public class WindowsOnlyAttribute : OperatingSystemAttribute
{
    /// <summary>
    ///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
    ///     <see cref="OSPlatform.Windows" />.
    /// </summary>
    public WindowsOnlyAttribute()
        : base(OSPlatform.Windows)
    {
    }
}