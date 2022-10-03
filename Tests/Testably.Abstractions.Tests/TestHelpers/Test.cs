using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class Test
{
    public static bool RunsOnWindows
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool RunsOnLinux
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}