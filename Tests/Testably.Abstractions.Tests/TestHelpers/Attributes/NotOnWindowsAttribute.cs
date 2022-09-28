using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

public class NotOnWindowsAttribute : OperatingSystemAttribute
{
    public NotOnWindowsAttribute()
        : base(OSPlatform.Linux, OSPlatform.OSX)
    {
    }
}