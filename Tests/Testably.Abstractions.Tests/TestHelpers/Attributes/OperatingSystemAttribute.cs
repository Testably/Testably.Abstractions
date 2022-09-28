using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

public class OperatingSystemAttribute : RuntimeSwitchAttribute
{
    public OperatingSystemAttribute(OSPlatform platform)
        : base(RuntimeInformation.IsOSPlatform(platform))
    {
    }

    public OperatingSystemAttribute(params OSPlatform[] platforms)
        : base(platforms.Any(RuntimeInformation.IsOSPlatform))
    {
    }
}