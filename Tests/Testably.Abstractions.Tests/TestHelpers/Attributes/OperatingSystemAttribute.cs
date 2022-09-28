using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
///     one of the platforms provided to the constructor.
/// </summary>
public class OperatingSystemAttribute : RuntimeSwitchAttribute
{
    /// <summary>
    ///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
    ///     the provided <paramref name="platform" />.
    /// </summary>
    public OperatingSystemAttribute(OSPlatform platform)
        : base(RuntimeInformation.IsOSPlatform(platform))
    {
    }

    /// <summary>
    ///     Enables tests only when <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)" /> is
    ///     any of the provided <paramref name="platforms" />.
    /// </summary>
    public OperatingSystemAttribute(params OSPlatform[] platforms)
        : base(platforms.Any(RuntimeInformation.IsOSPlatform))
    {
    }
}