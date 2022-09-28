using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
///     not <see cref="OSPlatform.Windows" />.
///     <para />
///     This means either <see cref="OSPlatform.Linux" /> or <see cref="OSPlatform.OSX" />.
/// </summary>
public class NotOnWindowsAttribute : OperatingSystemAttribute
{
    public NotOnWindowsAttribute()
        : base(OSPlatform.Linux, OSPlatform.OSX)
    {
    }
}