using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

public class WindowsOnlyAttribute : OperatingSystemAttribute
{
    public WindowsOnlyAttribute()
        : base(OSPlatform.Windows)
    {
    }
}