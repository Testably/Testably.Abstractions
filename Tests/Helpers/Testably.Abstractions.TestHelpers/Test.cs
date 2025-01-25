using System;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.TestHelpers;

public class Test
{
	public static bool IsNet8OrGreater
#if NET8_0_OR_GREATER
		=> true;
#else
		=> false;
#endif
	public bool IsNetFramework { get; }

	public bool RunsOnLinux { get; }

	public bool RunsOnMac { get; }

	public bool RunsOnWindows { get; }

	public Test()
	{
		RunsOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		RunsOnMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		RunsOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		IsNetFramework = RuntimeInformation.FrameworkDescription
			.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
	}

	public Test(OSPlatform osPlatform)
	{
		RunsOnLinux = osPlatform == OSPlatform.Linux;
		RunsOnMac = osPlatform == OSPlatform.OSX;
		RunsOnWindows = osPlatform == OSPlatform.Windows;
		IsNetFramework = false;
	}
}
