using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class Test
{
	private static bool? _isNetFramework;

	public static bool IsNetFramework
	{
		get
		{
			_isNetFramework ??= RuntimeInformation.FrameworkDescription
				.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
			return _isNetFramework.Value;
		}
	}

	public static bool RunsOnLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool RunsOnMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public static bool RunsOnWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
